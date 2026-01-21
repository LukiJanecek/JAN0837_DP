#!/usr/bin/env python3
# to project on path add Data type and from data type create DB
# terminal test: 
# withui: python addDBtoPathProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample" --ui
# withoutui: python addDBtoPathProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample"

import os
import sys
import argparse
import tempfile
from pathlib import Path
from collections import deque

import importTIADLL

def find_plc_software(dev):
    """Find PLC software on device or its children using generic GetService<T>() method"""
    from System.Reflection import Assembly
    
    try:
        asm = Assembly.Load("Siemens.Engineering")
        software_container_type = None
        for t in asm.GetTypes():
            if t.Name == "SoftwareContainer":
                software_container_type = t
                break
        
        if software_container_type:
            # Try on device first
            try:
                get_service_method = dev.GetType().GetMethod("GetService")
                if get_service_method and get_service_method.IsGenericMethodDefinition:
                    generic_method = get_service_method.MakeGenericMethod(software_container_type)
                    result = generic_method.Invoke(dev, None)
                    if result and hasattr(result, "Software"):
                        return dev, result.Software
            except Exception:
                pass
            
            # Try on device items
            try:
                for it in dev.DeviceItems:
                    get_service_method = it.GetType().GetMethod("GetService")
                    if get_service_method and get_service_method.IsGenericMethodDefinition:
                        generic_method = get_service_method.MakeGenericMethod(software_container_type)
                        result = generic_method.Invoke(it, None)
                        if result and hasattr(result, "Software"):
                            return it, result.Software
                    
                    # Also check nested items
                    try:
                        for child in it.DeviceItems:
                            get_service_method = child.GetType().GetMethod("GetService")
                            if get_service_method and get_service_method.IsGenericMethodDefinition:
                                generic_method = get_service_method.MakeGenericMethod(software_container_type)
                                result = generic_method.Invoke(child, None)
                                if result and hasattr(result, "Software"):
                                    return child, result.Software
                    except Exception:
                        pass
            except Exception:
                pass
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

def _delete_existing_external_source(src_group, name: str):
    """Delete existing ExternalSource with given name if it exists"""
    try:
        for es in list(src_group.ExternalSources):
            if str(es.Name) == name:
                es.Delete()
    except Exception:
        pass

def create_or_replace_udt(plc, type_name: str, fields: list = None) -> None:
    """Create/replace PLC data type (UDT) via TYPE/END_TYPE SCL"""
    from Siemens.Engineering.SW.Blocks import PlcBlock
    
    fields = fields or [("button", "Bool", "FALSE"),
                        ("LED", "Bool", "FALSE")]
    
    lines = []
    for (name, typ, init) in fields:
        t = str(typ).upper()
        if init is None or str(init) == "":
            lines.append(f"  {name:12s} : {t};")
        else:
            lines.append(f"  {name:12s} : {t} := {init};")
    body = "\n".join(lines)
    
    type_scl = f"""TYPE {type_name}
VERSION : 0.1
STRUCT
{body}
END_STRUCT
END_TYPE
"""
    
    # Delete existing UDT with same name
    for b in list(plc.BlockGroup.Blocks):
        if isinstance(b, PlcBlock) and str(b.Name) == type_name:
            b.Delete()
            break
    
    src_group = plc.ExternalSourceGroup
    src_name = f"TYPE_{type_name}.scl"
    
    # Remove conflicting external source
    _delete_existing_external_source(src_group, src_name)
    
    # Write temp file
    tmpfile = os.path.join(tempfile.gettempdir(), src_name)
    with open(tmpfile, "w", encoding="utf-8") as f:
        f.write(type_scl)
    
    try:
        src = src_group.ExternalSources.CreateFromFile(src_name, tmpfile)
        src.GenerateBlocksFromSource()
        # Keep project clean
        try:
            src.Delete()
        except Exception:
            pass
    finally:
        try:
            os.remove(tmpfile)
        except OSError:
            pass

def create_or_replace_simple_db(plc, db_name: str, udt_type: str = None, optimized: bool = True) -> None:
    """Create/replace simple DB via SCL external source"""
    from Siemens.Engineering.SW.Blocks import PlcBlock
    
    # Delete existing DB with same name
    for b in list(plc.BlockGroup.Blocks):
        if isinstance(b, PlcBlock) and str(b.Name) == db_name:
            b.Delete()
            break
    
    # Build SCL
    attr = "{ S7_Optimized_Access := 'TRUE' }" if optimized else "{ S7_Optimized_Access := 'FALSE' }"
    
    if udt_type:
        # Use UDT type
        db_scl = f"""DATA_BLOCK {db_name}
{attr}
VERSION : 0.1
  VAR
    data : {udt_type};
  END_VAR
BEGIN
END_DATA_BLOCK
"""
    else:
        # Simple variables
        db_scl = f"""DATA_BLOCK {db_name}
{attr}
VERSION : 0.1
  VAR
    Speed       : Real := 0.0;
    Count       : DInt := 0;
    Enabled     : Bool := FALSE;
  END_VAR
BEGIN
END_DATA_BLOCK
"""
    
    src_group = plc.ExternalSourceGroup
    
    # Normalize file name
    file_stub = db_name
    if file_stub.upper().startswith("DB_"):
        file_stub = file_stub[3:]
    src_name = f"DB_{file_stub}.scl"
    
    # Remove conflicting external source
    _delete_existing_external_source(src_group, src_name)
    
    # Write temp file
    tmpfile = os.path.join(tempfile.gettempdir(), src_name)
    with open(tmpfile, "w", encoding="utf-8") as f:
        f.write(db_scl)
    
    try:
        src = src_group.ExternalSources.CreateFromFile(src_name, tmpfile)
        src.GenerateBlocksFromSource()
        # Keep project clean
        try:
            src.Delete()
        except Exception:
            pass
    finally:
        try:
            os.remove(tmpfile)
        except OSError:
            pass

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
    
    print(f"[DEBUG] Input project path: {project_path}")

    # If it's a folder, look for the .ap19 file inside it
    if project_path.is_dir():
        print(f"[DEBUG] Path is a directory, searching for .ap19 file...")
        folder_name = project_path.name
        ap19_file = project_path / f"{folder_name}.ap19"
        if ap19_file.exists():
            project_path = ap19_file
            print(f"[OK] Found project file: {project_path}")
        else:
            ap19_files = list(project_path.glob("*.ap19"))
            if ap19_files:
                project_path = ap19_files[0]
                print(f"[OK] Found project file: {project_path}")
            else:
                print(f"[ERROR] No .ap19 file found in directory: {args.project_dir}")
                print(f"[DEBUG] Contents of {args.project_dir}:")
                for item in Path(args.project_dir).iterdir():
                    print(f"  - {item.name}")
                return

    # Check if project file exists
    if not project_path.exists():
        print(f"[ERROR] Project file not found: {project_path}")
        parent = project_path.parent
        print(f"[DEBUG] Contents of {parent}:")
        if parent.exists():
            for item in parent.iterdir():
                print(f"  - {item.name}")
        else:
            print(f"[ERROR] Parent directory does not exist: {parent}")
        return
    
    print(f"[OK] Project file exists: {project_path}")

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
    
    # Task 4: Create Data Type (UDT) and Data Block using SCL approach
    print("\n" + "=" * 60)
    print("Task 4: Creating Data Type (UDT) and Data Block")
    print("=" * 60)
    
    try:
        # Create UDT with custom fields
        print("[NOTE] Creating UDT 'MyDataType' with button and LED fields...")
        create_or_replace_udt(
            plc_sw, 
            "MyDataType",
            fields=[
                ("button", "Bool", "FALSE"),
                ("LED", "Bool", "FALSE")
            ]
        )
        print("[OK] UDT 'MyDataType' created successfully")
        
        # Create DB using the UDT
        print("[NOTE] Creating Data Block 'DB_ProcessData' using UDT...")
        create_or_replace_simple_db(
            plc_sw,
            "DB_ProcessData",
            udt_type="MyDataType",
            optimized=True
        )
        print("[OK] Data Block 'DB_ProcessData' created successfully")
        
    except Exception as e:
        print(f"[ERROR] Failed to create UDT or DB: {e}")
        import traceback
        traceback.print_exc()
        return
    
    # Task 5: Save and close project
    print("\n" + "=" * 60)
    print("Task 5: Saving project")
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

