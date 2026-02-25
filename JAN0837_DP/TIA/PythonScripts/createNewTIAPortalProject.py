#!/usr/bin/env python3
# Create a new TIA Portal project with PLC device, data type (UDT), and data block (DB).
#
# EXAMPLES (use forward slashes - easier in PowerShell):
#
# With UI:
# python createNewTIAPortalProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample" --project-name "JAN0837_test" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1" --ui
#
# Without UI (background mode):
# python createNewTIAPortalProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample" --project-name "JAN0837_test" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1"

import os 
import sys
import argparse
from pathlib import Path
from collections import deque

import tia_parameters as params
import tia_functions as fc

# Shared helpers moved to tia_functions.py

def main():
    parser = argparse.ArgumentParser(description="Create new TIA Portal project with data type and DB")
    parser.add_argument("--dll-dir", required=True, help="Directory containing TIA DLLs")
    parser.add_argument("--project-dir", required=True, help="Project directory path")
    parser.add_argument("--project-name", required=True, help="Project name")
    parser.add_argument("--type-id", required=True, help="PLC type ID")
    parser.add_argument("--plc-name", required=True, help="PLC/Device name")
    parser.add_argument("--ui", action="store_true", help="Open project with TIA Portal UI")
    
    args = parser.parse_args()
    
    # Task 1: import Siemens.Engineering.dll
    print("=" * 60)
    print("Task 1: Importing TIA DLL from", args.dll_dir)
    print("=" * 60)
    sys.argv = ["importTIADLL.py", "--dir", args.dll_dir]
    fc.import_tia_dll(args.dll_dir)

    import System
    from System.Reflection import Assembly, AssemblyName, ReflectionTypeLoadException
    from System import ResolveEventHandler, Enum
    from System.IO import FileInfo, DirectoryInfo
    from Siemens.Engineering import TiaPortal, TiaPortalMode
    
    # Task 2: create new TIA Portal project on path with given name
    print("\n" + "=" * 60)
    print("Task 2: Creating new TIA Portal project:", args.project_name)
    print("=" * 60)
    
    # Determine which mode to use
    mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface
    
    # Open the TIA Portal
    tia_portal = TiaPortal(mode)

    # Create parent directory if it doesn't exist
    project_dir_path = Path(args.project_dir)
    project_dir_path.mkdir(parents=True, exist_ok=True)
    
    # The project_dir is where TIA will CREATE the project subdirectory
    # Use it directly as the parent directory for TIA's Create method
    parent_dir = DirectoryInfo(str(project_dir_path))
    project_name = args.project_name
    
    # The actual project will be at: project_dir / project_name / project_name.ap19
    proj_folder = project_dir_path / project_name / f"{project_name}.ap19"
    
    # Open existing or create new project
    if proj_folder.exists():
        project = tia_portal.Projects.Open(FileInfo(str(proj_folder)))
        print(f"[OK] Project opened: {proj_folder}")
    else:
        # Check if project subdirectory exists and is not empty (to avoid TIA error)
        proj_subdir = project_dir_path / project_name
        if proj_subdir.exists() and any(proj_subdir.iterdir()):
            print(f"[WARN] Project subdirectory exists but is not empty: {proj_subdir}")
            print(f"[NOTE] Trying to open project anyway...")
            try:
                project = tia_portal.Projects.Open(FileInfo(str(proj_folder)))
                print(f"[OK] Project opened: {proj_folder}")
            except Exception as e:
                print(f"[ERROR] Could not open project: {e}")
                print(f"[NOTE] You may need to manually delete or clean: {proj_subdir}")
                return 4
        else:
            # Directory is empty or doesn't exist - safe to create
            project = tia_portal.Projects.Create(parent_dir, project_name)
            print(f"[OK] Project created: {project_name} in {args.project_dir}")
    
    print(f"[NOTE] Project file location: {proj_folder}")
    
    # Task 3: in project create new PLC with given type id and name
    print("\n" + "=" * 60)
    print("Task 3: Creating PLC:", args.plc_name, "with type:", args.type_id)
    print("=" * 60)
    
    # Check if device already exists
    device = None
    for dev in project.Devices:
        if dev.Name == args.plc_name:
            device = dev
            print(f"[OK] Found existing device: {device.Name}")
            break
    
    # If device doesn't exist, create it
    if device is None:
        print(f"[DEBUG] Creating device with name: {args.plc_name}")
        print(f"[DEBUG] Type ID: {args.type_id}")
        
        try:
            device = project.Devices.CreateWithItem(
                args.type_id, args.plc_name, args.plc_name
            )
            print(f"[OK] Device created successfully: {device.Name}")
        except Exception as e:
            print(f"[ERROR] Failed to create device: {e}")
            return 5
    
    # Get root item
    root_item = None
    try:
        for item in device.DeviceItems:
            root_item = item
            break
    except:
        pass
    
    if root_item:
        print(f"[OK] Root item obtained: {getattr(root_item, 'Name', '<root>')}")
    
    # Find the CPU item with SoftwareContainer
    # Note: We need to search from device, not from root_item!
    # The device has 2 DeviceItems: Rack_0 and PLC1
    # PLC1 is the one with SoftwareContainer
    
    cpu_item, plc_sw = fc.find_plc_software(device)
    if cpu_item is None or plc_sw is None:
        print(f"[ERROR] CPU item with SoftwareContainer not found")
        return 6
    
    print(f"[OK] Found CPU item: {getattr(cpu_item, 'Name', '<cpu>')}")
    print(f"[OK] PLC Software obtained: {plc_sw.GetType().FullName}")
    
    # Protection
    try:
        prot = fc.get_service_generic(cpu_item, "Protection")
        if prot:
            fc.set_enum_prop(prot, "Level", "FullAccess")
            fc.set_bool_prop(prot, "DownloadWithoutRewire", True)
            print("[OK] Protection set.")
        else:
            print("[WARN] Protection service not available.")
    except Exception as e:
        print(f"[WARN] Could not set protection: {e}")
    
    # StartInfo
    try:
        start_info = fc.get_service_generic(cpu_item, "StartInfo")
        if start_info:
            fc.set_enum_prop(start_info, "StartMode", "AlwaysRun")
            print("[OK] Start mode set.")
        else:
            print("[WARN] StartInfo service not available.")
    except Exception as e:
        print(f"[WARN] Could not set start mode: {e}")
    
    # Save project before creating blocks (TIA sometimes needs this)
    print("\n[NOTE] Saving project before creating blocks...")
    project.Save()
    print("[OK] Project saved (pre-block creation)")
    
    # Task 4: Create Data Type (UDT) and Data Block
    print("\n" + "=" * 60)
    print("Task 4: Creating Data Type (UDT) and Data Block")
    print("=" * 60)
    
    try:
        # Create UDT with custom fields - DEFAULT
        #print(f"[NOTE] Creating UDT '{params.DEFAULT_UDT_NAME}'...")
        #fc.create_or_replace_udt(
        #    plc_sw,
        #    params.DEFAULT_UDT_NAME,
        #    fields=params.DEFAULT_UDT_FIELDS,
        #)
        #print(f"[OK] UDT '{params.DEFAULT_UDT_NAME}' created successfully")
        
        # Create DB using the UDT - DEFAULT
        #print(f"[NOTE] Creating Data Block '{params.DEFAULT_DB_NAME}' using UDT...")
        #fc.create_or_replace_simple_db(
        #    plc_sw,
        #    params.DEFAULT_DB_NAME,
        #    udt_type=params.DEFAULT_UDT_NAME,
        #    optimized=params.DEFAULT_DB_OPTIMIZED,
        #)
        #print(f"[OK] Data Block '{params.DEFAULT_DB_NAME}' created successfully")

        # Crossroad PLC data type 
        print(f"[NOTE] Creating UDT '{params.Crossroad_Input_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.Crossroad_Input_UDT_NAME,
            fields=params.Crossroad_Input_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.Crossroad_Input_UDT_NAME}' created successfully")

        print(f"[NOTE] Creating UDT '{params.Crossroad_Output_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.Crossroad_Output_UDT_NAME,
            fields=params.Crossroad_Output_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.Crossroad_Output_UDT_NAME}' created successfully")

        # Crosswalk PLC data type 
        print(f"[NOTE] Creating UDT '{params.Crosswalk_Input_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.Crosswalk_Input_UDT_NAME,
            fields=params.Crosswalk_Input_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.Crosswalk_Input_UDT_NAME}' created successfully")

        print(f"[NOTE] Creating UDT '{params.Crosswalk_Output_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.Crosswalk_Output_UDT_NAME,
            fields=params.Crosswalk_Output_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.Crosswalk_Output_UDT_NAME}' created successfully")

        # Regulator PLC data type
        print(f"[NOTE] Creating UDT '{params.Regulator_Input_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.Regulator_Input_UDT_NAME,
            fields=params.Regulator_Input_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.Regulator_Input_UDT_NAME}' created successfully")

        print(f"[NOTE] Creating UDT '{params.Regulator_Output_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.Regulator_Output_UDT_NAME,
            fields=params.Regulator_Output_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.Regulator_Output_UDT_NAME}' created successfully")

        # CarWash PLC data type
        print(f"[NOTE] Creating UDT '{params.CarWash_Input_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.CarWash_Input_UDT_NAME,
            fields=params.CarWash_Input_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.CarWash_Input_UDT_NAME}' created successfully")

        print(f"[NOTE] Creating UDT '{params.CarWash_Output_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.CarWash_Output_UDT_NAME,
            fields=params.CarWash_Output_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.CarWash_Output_UDT_NAME}' created successfully")

        # WashingMachine PLC data type
        print(f"[NOTE] Creating UDT '{params.WashingMachine_Input_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.WashingMachine_Input_UDT_NAME,
            fields=params.WashingMachine_Input_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.WashingMachine_Input_UDT_NAME}' created successfully")

        print(f"[NOTE] Creating UDT '{params.WashingMachine_Output_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.WashingMachine_Output_UDT_NAME,
            fields=params.WashingMachine_Output_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.WashingMachine_Output_UDT_NAME}' created successfully")

        # CarLight PLC data type
        print(f"[NOTE] Creating UDT '{params.CarLight_Input_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.CarLight_Input_UDT_NAME,
            fields=params.CarLight_Input_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.CarLight_Input_UDT_NAME}' created successfully")

        print(f"[NOTE] Creating UDT '{params.CarLight_Output_UDT_NAME}'...")
        fc.create_or_replace_udt(
            plc_sw,
            params.CarLight_Output_UDT_NAME,
            fields=params.CarLight_Output_UDT_FIELDS,
        )
        print(f"[OK] UDT '{params.CarLight_Output_UDT_NAME}' created successfully")

        # Create DB with both UDTs
        print(f"[NOTE] Creating Data Block '{params.DEFAULT_DB_NAME}' with input and output UDTs...")
        fc.create_or_replace_db_with_multiple_udts(
            plc_sw,
            params.DEFAULT_DB_NAME,
            udt_vars=[
                ("input", params.CarLight_Input_UDT_NAME),
                ("output", params.CarLight_Output_UDT_NAME),
            ],
            optimized=params.DEFAULT_DB_OPTIMIZED,
        )
        print(f"[OK] Data Block '{params.DEFAULT_DB_NAME}' created successfully")
        
    except Exception as e:
        print(f"[ERROR] Failed to create UDT or DB: {e}")
        import traceback
        traceback.print_exc()
        return 7
    
    # Task 5: Save project
    print("\n" + "=" * 60)
    print("Task 5: Saving project")
    print("=" * 60)

    project.Save()
    print("[SUCCESS] Project saved.")

    # Close if in background mode
    fc.close_if_headless(None, tia_portal, mode)
    if mode == TiaPortalMode.WithUserInterface:
        print("[NOTE] Project remains open in TIA Portal UI.")

    return 0

if __name__ == "__main__":
    rc = main()
    sys.exit(rc if isinstance(rc, int) else 1)




