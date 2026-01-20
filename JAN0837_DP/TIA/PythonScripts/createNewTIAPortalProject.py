#!/usr/bin/env python3
# to project on path add Data type and from data type create DB

# terminal test: 
# withui: python createNewTIAPortalProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\" --project-name "JAN0837_test" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1" --ui
# python createNewTIAPortalProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample" --project-name "JAN0837_test2" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1" --ui
# withoutui: python createNewTIAPortalProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\" --project-name "JAN0837_test" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1"

import os 
import sys
import argparse
from pathlib import Path
from collections import deque

import importTIADLL

def find_plc_software(dev):
    # zkus přímo na zařízení
    try:
        sc = dev.GetService("SoftwareContainer")
        if sc is not None and getattr(sc, "Software", None) is not None:
            return dev, sc.Software
    except Exception:
        pass
    # fallback: jakákoli služba vracející SW
    try:
        for svc in dev.Services:
            st = svc.GetType()
            # pokud má property 'Software' končící na PlcSoftware, použij ji
            p = st.GetProperty("Software")
            if p:
                sw = p.GetValue(svc, None)
                if sw is not None and sw.GetType().Name.endswith("PlcSoftware"):
                    return dev, sw
    except Exception:
        pass
    # projdi strom itemů
    q = deque()
    try:
        for it in dev.DeviceItems:
            q.append(it)
    except Exception:
        pass

    while q:
        it = q.popleft()
        # varianta se SoftwareContainer
        try:
            sc = it.GetService("SoftwareContainer")
            if sc is not None and getattr(sc, "Software", None) is not None:
                return it, sc.Software
        except Exception:
            pass
        # varianta s property Software na jiné službě
        try:
            for svc in it.Services:
                p = svc.GetType().GetProperty("Software")
                if p:
                    sw = p.GetValue(svc, None)
                    if sw is not None and sw.GetType().Name.endswith("PlcSoftware"):
                        return it, sw
        except Exception:
            pass
        # enqueue children
        try:
            for ch in it.DeviceItems:
                q.append(ch)
        except Exception:
            pass
    return None, None

def set_enum_prop(obj, prop_name, value_name):
    if obj is None: return False
    p = obj.GetType().GetProperty(prop_name)
    if p is None or not p.PropertyType.IsEnum: return False
    v = Enum.Parse(p.PropertyType, value_name)
    p.SetValue(obj, v, None); return True

def set_bool_prop(obj, prop_name, value: bool):
    if obj is None: return False
    p = obj.GetType().GetProperty(prop_name)
    if p is None or str(p.PropertyType.FullName) != "System.Boolean": return False
    p.SetValue(obj, value, None); return True

def find_enum(simple_name: str):
    for t in _TYPES:
        if t is not None and t.IsEnum and t.Name == simple_name:
            return t
    return None

def enum_val(enum_type, name: str):
    return Enum.Parse(enum_type, name)

def create_var_with_datatype(static_iface, var_name: str, datatype_name: str) -> bool:
    t = static_iface.GetType()
    for m in t.GetMethods():
        if m.Name != "Create": continue
        pars = m.GetParameters()
        if len(pars) != 2: continue
        if str(pars[0].ParameterType.FullName) != "System.String": continue
        dt = pars[1].ParameterType
        if dt.IsEnum and dt.Name == "DataType":
            v = Enum.Parse(dt, datatype_name)
            static_iface.Create(var_name, v)
            return True
    return False

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
    importTIADLL.main()

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
                return
        else:
            # Directory is empty or doesn't exist - safe to create
            project = tia_portal.Projects.Create(parent_dir, project_name)
            print(f"[OK] Project created: {project_name} in {args.project_dir}")
    
    print(f"[NOTE] Project file location: {proj_folder}")
    
    # Task 3: in project create new PLC with given type id and name
    print("\n" + "=" * 60)
    print("Task 3: Creating PLC:", args.plc_name, "with type:", args.type_id)
    print("=" * 60)
    
    print(f"[DEBUG] Creating device with name: {args.plc_name}")
    print(f"[DEBUG] Type ID: {args.type_id}")
    
    try:
        device = project.Devices.CreateWithItem(
            args.type_id, args.plc_name, args.plc_name
        )
        print(f"[OK] Device created successfully")
    except Exception as e:
        print(f"[ERROR] Failed to create device with name '{args.plc_name}'")
        print(f"[DEBUG] Error: {e}")
        print(f"\n[NOTE] Trying alternative: Using 'Device' as name...")
        try:
            device = project.Devices.CreateWithItem(
                args.type_id, "Device", "Device"
            )
            print(f"[OK] Device created with default name 'Device'")
        except Exception as e2:
            print(f"[ERROR] Failed with default name too: {e2}")
            return
    
    print(f"[DEBUG] Device created: {device.Name}")
    print(f"[DEBUG] Device type: {device.GetType().FullName}")
    
    # Try to access services, but handle if not available
    try:
        services = list(device.Services) if hasattr(device, 'Services') else []
        print(f"[DEBUG] Device services: {[s.GetType().Name for s in services]}")
    except Exception as e:
        print(f"[DEBUG] Could not list services: {e}")
    
    try:
        items = list(device.DeviceItems) if hasattr(device, 'DeviceItems') else []
        print(f"[DEBUG] Device items: {[item.Name for item in items]}")
    except Exception as e:
        print(f"[DEBUG] Could not list items: {e}")
    
    # SAVE PROJECT BEFORE ACCESSING PLC SOFTWARE
    print("\n[NOTE] Saving project to initialize PLC software...")
    try:
        project.Save()
        print("[OK] Project saved.")
    except Exception as e:
        print(f"[WARN] Could not save project: {e}")

    cpu_item, plc_sw = find_plc_software(device)
    if plc_sw is None:
        print(f"\n[ERROR] Could not find PLC software using Services approach")
        print(f"[DEBUG] Exploring full device tree structure:")
        print(f"  - Device Name: {device.Name}")
        print(f"  - Device Type: {device.GetType().FullName}")
        
        # List all properties and methods to understand the structure
        try:
            device_type = device.GetType()
            print(f"\n[DEBUG] Device properties:")
            for prop in device_type.GetProperties():
                try:
                    value = prop.GetValue(device, None)
                    print(f"  - {prop.Name}: {value.GetType().Name if value else 'None'}")
                except:
                    pass
        except Exception as e:
            print(f"  - Could not list properties: {e}")
        
        # Deep dive into items
        try:
            print(f"\n[DEBUG] Device items and their structure:")
            for item in device.DeviceItems:
                print(f"  - Item: {item.Name} ({item.GetType().Name})")
                item_type = item.GetType()
                for prop in item_type.GetProperties():
                    try:
                        value = prop.GetValue(item, None)
                        if value and "Software" in prop.Name:
                            print(f"    - {prop.Name}: {value.GetType().Name}")
                    except:
                        pass
        except Exception as e:
            print(f"  - Could not explore items: {e}")
        
        raise RuntimeError("Could not find PLC software.\n" +
                        "The device appears to be created, but software access method is unknown.\n" +
                        "Check the debug output above and the TIA Portal project manually.")

    print(f"[OK] Našel jsem PLC software na: {getattr(cpu_item, 'Name', '<device>')}  ({plc_sw.GetType().FullName})")

    # Protection
    protection = cpu_item.GetService("Protection")
    if protection:
        set_enum_prop(protection, "Level", "FullAccess")
        set_bool_prop(protection, "DownloadWithoutRewire", True)
        print("[OK] Protection set.")
    else:
        print("[WARN] Protection service not available.")

    # StartInfo
    start_info = cpu_item.GetService("StartInfo")
    if start_info:
        set_enum_prop(start_info, "StartMode", "AlwaysRun")
        print("[OK] Start mode set.")
    else:
        print("[WARN] StartInfo service not available.")

    # PLC Software 
    swc = cpu_item.GetService("SoftwareContainer")
    if swc is None: raise RuntimeError("SoftwareContainer not found on CPU item.")
    plc_sw = swc.Software

    PlcBlockType = find_enum("PlcBlockType")
    PlcProgrammingLanguage = find_enum("PlcProgrammingLanguage")
    if PlcBlockType is None or PlcProgrammingLanguage is None:
        raise RuntimeError("Cannot resolve PlcBlockType or PlcProgrammingLanguage.")
    
    # Task 4: Create custom data type in PLC
    print("\n" + "=" * 60)
    print("Task 4: Creating custom data type in PLC")
    print("=" * 60)
    
    # Create a custom data type with multiple fields
    user_datatypes = plc_sw.TypeGroup.UserDefinedTypes
    custom_datatype = user_datatypes.Create("OrderData")
    
    # Add members to custom data type
    try:
        if create_var_with_datatype(custom_datatype.BaseType.Members, "OrderID", "DWord"):
            print("[OK] Custom DataType field 'OrderID' (DWORD) created.")
        if create_var_with_datatype(custom_datatype.BaseType.Members, "OrderValue", "Real"):
            print("[OK] Custom DataType field 'OrderValue' (REAL) created.")
        if create_var_with_datatype(custom_datatype.BaseType.Members, "CustomerName", "String"):
            print("[OK] Custom DataType field 'CustomerName' (STRING) created.")
    except Exception as e:
        print(f"[WARN] Could not add all fields to custom data type: {e}")
    
    print("[OK] Custom DataType 'OrderData' created successfully.")
    
    # Task 5: Create DB in PLC with custom data type
    print("\n" + "=" * 60)
    print("Task 5: Creating DB in PLC with custom data type")
    print("=" * 60)

    db = plc_sw.BlockGroup.Blocks.Create(
        enum_val(PlcBlockType, "DataBlock"),
        "MyDataBlock",
        enum_val(PlcProgrammingLanguage, "LAD")
    )

    # Add variable of custom data type to DB
    if create_var_with_datatype(db.Interface.Static, "myOrder", "OrderData"):
        print("[OK] DB variable 'myOrder' with custom type 'OrderData' created.")
    else:
        print("[WARN] Could not add custom data type variable to DB.")
    
    # Also add a simple variable for reference
    if create_var_with_datatype(db.Interface.Static, "myCounter", "Int"):
        print("[OK] DB variable 'myCounter' (INT) created.")
    else:
        print("[WARN] Could not add simple variable to DB.")
    
    # Task 6: save project 
    print("\n" + "=" * 60)
    print("Task 4: Saving project")
    print("=" * 60)

    project.Save()
    print("[SUCCESS] Project saved.")

    # Close the project and portal
    if mode == TiaPortalMode.WithoutUserInterface:
        project.Close()
        tia_portal.Dispose()

if __name__ == "__main__":
    main()


