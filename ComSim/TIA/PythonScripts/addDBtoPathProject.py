#!/usr/bin/env python3
# Add data type (UDT) and data block (DB) to an existing TIA Portal project.
#
# EXAMPLES (use forward slashes - easier in PowerShell):
#
# With UI:
# python addDBtoPathProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample/JAN0837_sample" --ui
#
# Without UI (background mode):
# python addDBtoPathProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample/JAN0837_sample"

import os
import sys
import argparse
from pathlib import Path

import tia_parameters as params
import tia_functions as fc

def main():
    parser = argparse.ArgumentParser(description="Add DB to TIA Portal project")
    parser.add_argument("--dll-dir", required=True, help="Directory containing TIA DLLs")
    parser.add_argument("--project-dir", required=True, help="Project directory path")
    parser.add_argument("--ui", action="store_true", help="Open project with TIA Portal UI")
    
    args = parser.parse_args()
    
    # Task 1: import Siemens.Engineering.dll
    # import Siemens.Engineering.dll
    print("=" * 60)
    print("Task 1: Importing TIA DLL from", args.dll_dir)
    print("=" * 60)
    sys.argv = ["importTIADLL.py", "--dir", args.dll_dir]
    import_result = fc.import_tia_dll(args.dll_dir)
    if import_result != 0:
        return import_result

    from Siemens.Engineering import TiaPortalMode

    # Task 2: open project on path
    print("\n" + "=" * 60)
    print("Task 2: Opening TIA Portal project at", args.project_dir)
    print("=" * 60)

    # Open the TIA Portal
    try:
        tia_portal, mode = fc.open_tia_portal(args.ui)
        print("[OK] TIA Portal opened in", "UI mode" if mode == TiaPortalMode.WithUserInterface else "background mode")
    except Exception as e:
        print(f"[ERROR] Could not open TIA Portal: {e}")
        return 2

    # Handle project path
    try:
        project_path = fc.locate_project_file(args.project_dir)
        print(f"[OK] Project file: {project_path}")
    except FileNotFoundError as e:
        print(f"[ERROR] {e}")
        return 3

    try:
        project = fc.open_project(tia_portal, project_path)
        print(f"[OK] Project opened successfully: {project_path}")
        print(f"Project name: {project.Name}")
    except Exception as e:
        print(f"[ERROR] Could not open project: {e}")
        return 4

# Task 3: Find PLC and its software
    print("\n" + "=" * 60)
    print("Task 3: Finding PLC software in project")
    print("=" * 60)
    
    plc_sw = None
    for device in project.Devices:
        cpu_item, plc_sw = fc.find_plc_software(device)
        if plc_sw is not None:
            print(f"[OK] Found PLC software on device: {device.Name}")
            break

    if plc_sw is None:
        print("[ERROR] No PLC software found in project")
        project.Close()
        tia_portal.Dispose()
        return 5
    
    # Task 4: Create Data Type (UDT) and Data Block using SCL approach
    print("\n" + "=" * 60)
    print("Task 4: Creating Data Type (UDT) and Data Block")
    print("=" * 60)
    
    try:
        exercise_udts = [
            ("crossroad", params.Crossroad_Input_UDT_NAME, params.Crossroad_Input_UDT_FIELDS, params.Crossroad_Output_UDT_NAME, params.Crossroad_Output_UDT_FIELDS),
            ("crosswalk", params.Crosswalk_Input_UDT_NAME, params.Crosswalk_Input_UDT_FIELDS, params.Crosswalk_Output_UDT_NAME, params.Crosswalk_Output_UDT_FIELDS),
            ("regulator", params.Regulator_Input_UDT_NAME, params.Regulator_Input_UDT_FIELDS, params.Regulator_Output_UDT_NAME, params.Regulator_Output_UDT_FIELDS),
            #("carwash", params.CarWash_Input_UDT_NAME, params.CarWash_Input_UDT_FIELDS, params.CarWash_Output_UDT_NAME, params.CarWash_Output_UDT_FIELDS),
            #("washingmachine", params.WashingMachine_Input_UDT_NAME, params.WashingMachine_Input_UDT_FIELDS, params.WashingMachine_Output_UDT_NAME, params.WashingMachine_Output_UDT_FIELDS),
            ("carlight", params.CarLight_Input_UDT_NAME, params.CarLight_Input_UDT_FIELDS, params.CarLight_Output_UDT_NAME, params.CarLight_Output_UDT_FIELDS),
        ]

        db_udt_vars = []

        for prefix, input_udt_name, input_udt_fields, output_udt_name, output_udt_fields in exercise_udts:
            print(f"[NOTE] Creating UDT '{input_udt_name}'...")
            fc.create_or_replace_udt(
                plc_sw,
                input_udt_name,
                fields=input_udt_fields,
            )
            print(f"[OK] UDT '{input_udt_name}' created successfully")

            print(f"[NOTE] Creating UDT '{output_udt_name}'...")
            fc.create_or_replace_udt(
                plc_sw,
                output_udt_name,
                fields=output_udt_fields,
            )
            print(f"[OK] UDT '{output_udt_name}' created successfully")

            db_udt_vars.append((f"{prefix}_input", input_udt_name))
            db_udt_vars.append((f"{prefix}_output", output_udt_name))

        print(f"[NOTE] Creating Data Block '{params.DEFAULT_DB_NAME}' with all exercise input/output UDTs...")
        fc.create_or_replace_db_with_multiple_udts(
            plc_sw,
            params.DEFAULT_DB_NAME,
            udt_vars=db_udt_vars,
            optimized=params.DEFAULT_DB_OPTIMIZED,
        )
        print(f"[OK] Data Block '{params.DEFAULT_DB_NAME}' created successfully")
        
    except Exception as e:
        print(f"[ERROR] Failed to create UDT or DB: {e}")
        import traceback
        traceback.print_exc()
        return 6
    
    # Task 5: Save and close project
    print("\n" + "=" * 60)
    print("Task 5: Saving project")
    print("=" * 60)

    try:
        project.Save()
        print("[SUCCESS] Project saved.")
    except Exception as e:
        print(f"[ERROR] Could not save project: {e}")
        return 7

    # Close if in background mode
    fc.close_if_headless(project, tia_portal, mode)
    if mode == TiaPortalMode.WithUserInterface:
        print("[NOTE] Project remains open in TIA Portal UI.")

    return 0

if __name__ == "__main__":
    rc = main()
    sys.exit(rc if isinstance(rc, int) else 1)

