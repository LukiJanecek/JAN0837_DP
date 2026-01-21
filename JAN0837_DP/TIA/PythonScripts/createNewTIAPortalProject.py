#!/usr/bin/env python3
# to project on path add Data type and from data type create DB

# terminal test: 
# withui: python createNewTIAPortalProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\" --project-name "JAN0837_test" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1" --ui
# python createNewTIAPortalProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample" --project-name "JAN0837_test2" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1" --ui
# withoutui: python createNewTIAPortalProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\" --project-name "JAN0837_test" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1"

import os 
import sys
import argparse
import tempfile
from pathlib import Path
from collections import deque

import importTIADLL

# Global type cache for find_enum
_TYPES = None

def _init_types():
    """Initialize global type cache from Siemens.Engineering assembly"""
    global _TYPES
    if _TYPES is not None:
        return
    
    from System.Reflection import Assembly, ReflectionTypeLoadException
    _TYPES = []
    try:
        # Load main assembly
        asm = Assembly.Load("Siemens.Engineering")
        try:
            _TYPES.extend(list(asm.GetTypes()))
        except ReflectionTypeLoadException as e:
            _TYPES.extend([t for t in e.Types if t is not None])
    except Exception:
        pass

def find_plc_software(dev):
    from System.Reflection import Assembly
    
    # Try using generic GetService<T>() method
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

def get_service_generic(item, service_type_name):
    """Get service using generic method invocation with GetService<T>()"""
    from System.Reflection import Assembly
    
    try:
        asm = Assembly.Load("Siemens.Engineering")
        service_type = None
        for t in asm.GetTypes():
            if t.Name == service_type_name:
                service_type = t
                break
        
        if service_type:
            get_service_method = item.GetType().GetMethod("GetService")
            if get_service_method and get_service_method.IsGenericMethodDefinition:
                generic_method = get_service_method.MakeGenericMethod(service_type)
                result = generic_method.Invoke(item, None)
                return result
    except Exception:
        pass
    
    return None

def set_enum_prop(obj, prop_name, value_name):
    from System import Enum
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
    _init_types()
    for t in _TYPES:
        if t is not None and t.IsEnum and t.Name == simple_name:
            return t
    return None

def enum_val(enum_type, name: str):
    from System import Enum
    return Enum.Parse(enum_type, name)

def find_cpu_item(root_item):
    """Find CPU item with SoftwareContainer service in device tree"""
    from collections import deque
    queue = deque([root_item])
    while queue:
        it = queue.popleft()
        try:
            if it.GetService("SoftwareContainer") is not None:
                return it
        except:
            pass
        try:
            for ch in it.DeviceItems:
                queue.append(ch)
        except:
            pass
    return None

def create_var_with_datatype(static_iface, var_name: str, datatype_name: str) -> bool:
    from System import Enum
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
            return
    
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
    
    cpu_item, plc_sw = find_plc_software(device)
    if cpu_item is None or plc_sw is None:
        print(f"[ERROR] CPU item with SoftwareContainer not found")
        return
    
    print(f"[OK] Found CPU item: {getattr(cpu_item, 'Name', '<cpu>')}")
    print(f"[OK] PLC Software obtained: {plc_sw.GetType().FullName}")
    
    # Protection
    try:
        prot = get_service_generic(cpu_item, "Protection")
        if prot:
            set_enum_prop(prot, "Level", "FullAccess")
            set_bool_prop(prot, "DownloadWithoutRewire", True)
            print("[OK] Protection set.")
        else:
            print("[WARN] Protection service not available.")
    except Exception as e:
        print(f"[WARN] Could not set protection: {e}")
    
    # StartInfo
    try:
        start_info = get_service_generic(cpu_item, "StartInfo")
        if start_info:
            set_enum_prop(start_info, "StartMode", "AlwaysRun")
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
        # Create UDT with button input and LED output
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
    
    # Task 5: Save project
    print("\n" + "=" * 60)
    print("Task 5: Saving project")
    print("=" * 60)

    project.Save()
    print("[SUCCESS] Project saved.")

    # Close the project and portal
    if mode == TiaPortalMode.WithoutUserInterface:
        project.Close()
        tia_portal.Dispose()

if __name__ == "__main__":
    main()




