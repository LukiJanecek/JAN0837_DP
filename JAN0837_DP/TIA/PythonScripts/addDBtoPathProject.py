#!/usr/bin/env python3
# to project on path add Data type and from data type create DB
# terminal test: 
# withui: python addDBtoPathProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample" --ui
# withoutui: python addDBtoPathProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample"

import sys
import argparse
from pathlib import Path
from collections import deque

import importTIADLL

def find_plc_software(dev):
    """Find PLC software on device or its children"""
    try:
        sc = dev.GetService("SoftwareContainer")
        if sc is not None and getattr(sc, "Software", None) is not None:
            return dev, sc.Software
    except Exception:
        pass
    
    try:
        for svc in dev.Services:
            st = svc.GetType()
            p = st.GetProperty("Software")
            if p:
                sw = p.GetValue(svc, None)
                if sw is not None and sw.GetType().Name.endswith("PlcSoftware"):
                    return dev, sw
    except Exception:
        pass
    
    q = deque()
    try:
        for it in dev.DeviceItems:
            q.append(it)
    except Exception:
        pass

    while q:
        it = q.popleft()
        try:
            sc = it.GetService("SoftwareContainer")
            if sc is not None and getattr(sc, "Software", None) is not None:
                return it, sc.Software
        except Exception:
            pass
        try:
            for svc in it.Services:
                p = svc.GetType().GetProperty("Software")
                if p:
                    sw = p.GetValue(svc, None)
                    if sw is not None and sw.GetType().Name.endswith("PlcSoftware"):
                        return it, sw
        except Exception:
            pass
        try:
            for ch in it.DeviceItems:
                q.append(ch)
        except Exception:
            pass
    
    return None, None

def find_enum(simple_name: str):
    """Find enum type by name"""
    for t in _TYPES:
        if t is not None and t.IsEnum and t.Name == simple_name:
            return t
    return None

def enum_val(enum_type, name: str):
    """Parse enum value by name"""
    return Enum.Parse(enum_type, name)

def create_var_with_datatype(static_iface, var_name: str, datatype_name: str) -> bool:
    """Create variable with specified data type"""
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
    importTIADLL.main()

    import System
    from System.Reflection import Assembly, AssemblyName, ReflectionTypeLoadException
    from System import ResolveEventHandler, Enum
    from System.IO import FileInfo
    from Siemens.Engineering import TiaPortal, TiaPortalMode

    # Task 2: open project on path
    print("\n" + "=" * 60)
    print("Task 2: Opening TIA Portal project at", args.project_dir)
    print("=" * 60)

    # Determine which mode to use
    mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface

    # Open the TIA Portal
    try:
        tia_portal = TiaPortal(mode)
        print("[OK] TIA Portal opened in", "UI mode" if args.ui else "background mode")
    except Exception as e:
        print(f"[ERROR] Could not open TIA Portal: {e}")
        print("[NOTE] Project might already be open in TIA Portal. Trying to access it...")
        # Try to get the already open portal
        try:
            tia_portal = TiaPortal(TiaPortalMode.WithUserInterface)
        except Exception as e2:
            print(f"[ERROR] Could not access TIA Portal: {e2}")
            return

    # Handle project path
    project_path = Path(args.project_dir)

    # If it's a folder, look for the .ap19 file inside it
    if project_path.is_dir():
        folder_name = project_path.name
        ap19_file = project_path / f"{folder_name}.ap19"
        if ap19_file.exists():
            project_path = ap19_file
        else:
            ap19_files = list(project_path.glob("*.ap19"))
            if ap19_files:
                project_path = ap19_files[0]

    # Check if project file exists
    if not project_path.exists():
        print(f"[ERROR] Project file not found: {project_path}")
        parent = project_path.parent if project_path.is_file() else project_path
        print(f"[DEBUG] Contents of {parent}:")
        if parent.exists():
            for item in parent.iterdir():
                print(f"  - {item.name}")
        return

    # Debug: show what we're opening
    print(f"[DEBUG] Opening project file: {project_path}")

    # Open the project
    try:
        project = tia_portal.Projects.Open(FileInfo(str(project_path)))
        print(f"[OK] Project opened successfully: {project_path}")
        print(f"Project name: {project.Name}")
    except Exception as e:
        print(f"[ERROR] Could not open project: {e}")
        return

# Task 3: Find PLC and its software
    print("\n" + "=" * 60)
    print("Task 3: Finding PLC software in project")
    print("=" * 60)
    
    plc_sw = None
    for device in project.Devices:
        cpu_item, plc_sw = find_plc_software(device)
        if plc_sw is not None:
            print(f"[OK] Found PLC software on device: {device.Name}")
            break
    
    if plc_sw is None:
        print("[ERROR] No PLC software found in project")
        project.Close()
        tia_portal.Dispose()
        return
    
    # Task 4: Create custom data type in PLC
    print("\n" + "=" * 60)
    print("Task 4: Creating custom data type in PLC")
    print("=" * 60)
    
    try:
        # Find enums
        PlcBlockType = find_enum("PlcBlockType")
        PlcProgrammingLanguage = find_enum("PlcProgrammingLanguage")
        
        if PlcBlockType is None or PlcProgrammingLanguage is None:
            print("[WARN] Could not resolve block type enums")
        
        # Create custom data type
        user_datatypes = plc_sw.TypeGroup.UserDefinedTypes
        custom_datatype = user_datatypes.Create("OrderData")
        
        # Add members to custom data type
        if create_var_with_datatype(custom_datatype.BaseType.Members, "OrderID", "DWord"):
            print("[OK] Custom DataType field 'OrderID' (DWORD) created.")
        if create_var_with_datatype(custom_datatype.BaseType.Members, "OrderValue", "Real"):
            print("[OK] Custom DataType field 'OrderValue' (REAL) created.")
        if create_var_with_datatype(custom_datatype.BaseType.Members, "CustomerName", "String"):
            print("[OK] Custom DataType field 'CustomerName' (STRING) created.")
        
        print("[OK] Custom DataType 'OrderData' created successfully.")
    except Exception as e:
        print(f"[WARN] Could not create custom data type: {e}")
    
    # Task 5: Create DB in PLC with custom data type
    print("\n" + "=" * 60)
    print("Task 5: Creating DB in PLC with custom data type")
    print("=" * 60)
    
    try:
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
        
        # Add simple variable for reference
        if create_var_with_datatype(db.Interface.Static, "myCounter", "Int"):
            print("[OK] DB variable 'myCounter' (INT) created.")
        else:
            print("[WARN] Could not add simple variable to DB.")
        
        print("[OK] DataBlock 'MyDataBlock' created successfully.")
    except Exception as e:
        print(f"[ERROR] Could not create data block: {e}")
    
    # Task 6: Save and close project
    print("\n" + "=" * 60)
    print("Task 6: Saving project")
    print("=" * 60)

    try:
        project.Save()
        print("[SUCCESS] Project saved.")
    except Exception as e:
        print(f"[ERROR] Could not save project: {e}")

    # Close the project and portal only if opened without UI
    if mode == TiaPortalMode.WithoutUserInterface:
        try:
            project.Close()
            print("[OK] Project closed.")
        except Exception as e:
            print(f"[WARN] Could not close project: {e}")
        
        try:
            tia_portal.Dispose()
            print("[OK] TIA Portal closed.")
        except Exception as e:
            print(f"[WARN] Could not dispose TIA Portal: {e}")
    else:
        print("[NOTE] Project remains open in TIA Portal UI.")

if __name__ == "__main__":
    main()

