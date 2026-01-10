# this doesnt work properly

# createTIAtemplate.py  (TIA V19, pythonnet 3.x)
import os, sys, clr
from pathlib import Path
import argparse

# ---------- 1) Najdi TIA V19 a připrav loader ----------
def find_tia_root():
    try:
        import winreg
        for keypath in [
            r"SOFTWARE\Siemens\Automation\TIAPortal\V19",
            r"SOFTWARE\WOW6432Node\Siemens\Automation\TIAPortal\V19",
        ]:
            try:
                with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, keypath) as k:
                    val, _ = winreg.QueryValueEx(k, "InstallationPath")
                    if val and os.path.isdir(val):
                        return val
            except FileNotFoundError:
                pass
    except Exception:
        pass
    for cand in [
        r"C:\Program Files\Siemens\Automation\Portal V19",
        r"C:\Program Files\Siemens\Automation\Portal V19.0",
    ]:
        if os.path.isdir(cand):
            return cand
    raise RuntimeError("TIA Portal V19 installation not found.")

TIA_ROOT = find_tia_root()
TIA_PUBLIC_V19 = os.path.join(TIA_ROOT, "PublicAPI", "V19")
TIA_BIN = os.path.join(TIA_ROOT, "Bin")

# Přidej složky do resolveru (native + managed)
if hasattr(os, "add_dll_directory"):
    for d in [TIA_PUBLIC_V19, TIA_BIN]:
        if os.path.isdir(d): os.add_dll_directory(d)
for d in [TIA_PUBLIC_V19, TIA_BIN]:
    if os.path.isdir(d): sys.path.append(d)

import System
from System.Reflection import Assembly, AssemblyName, ReflectionTypeLoadException
from System import ResolveEventHandler, Enum

_SEARCH_ROOTS = [Path(TIA_PUBLIC_V19), Path(TIA_BIN), Path(TIA_ROOT)]
_CACHE = {}

def _find_dll(name_no_ext: str) -> Path | None:
    p = _CACHE.get(name_no_ext)
    if p and p.is_file(): return p
    # rychlé pokusy
    for root in _SEARCH_ROOTS[:-1]:
        cand = root / f"{name_no_ext}.dll"
        if cand.is_file():
            _CACHE[name_no_ext] = cand
            return cand
    # fallback: rekurzivně v TIA_ROOT (jednou)
    for p in _SEARCH_ROOTS[-1].rglob(f"{name_no_ext}.dll"):
        _CACHE[name_no_ext] = p
        return p
    return None

def _resolve(sender, args):
    name = AssemblyName(args.Name).Name
    dll = _find_dll(name)
    if dll:
        try:
            return Assembly.LoadFrom(str(dll))
        except Exception:
            pass
    return None

_resolver_delegate = ResolveEventHandler(_resolve)
System.AppDomain.CurrentDomain.add_AssemblyResolve(_resolver_delegate)

# ---------- 2) Načti TIA assembly ----------
clr.AddReference("Siemens.Engineering")
asm = Assembly.Load("Siemens.Engineering")

def _all_types(a):
    try:
        return list(a.GetTypes())
    except ReflectionTypeLoadException as e:
        return [t for t in e.Types if t is not None]

# ---------- 3) Pomocné funkce pro enumy přes reflexi ----------
def get_enum_type(obj, prop_name):
    """Z objektu služby vezmi typ vlastnosti (pokud je Enum)."""
    if obj is None: return None
    t = obj.GetType()
    p = t.GetProperty(prop_name)
    if p is None: return None
    et = p.PropertyType
    return et if et.IsEnum else None

def set_enum_by_name(obj, prop_name, value_name):
    """Nastav enum vlastnost podle názvu hodnoty (bez ohledu na namespace)."""
    et = get_enum_type(obj, prop_name)
    if et is None:
        print(f"[WARN] Property '{prop_name}' is not an enum or not found.")
        return False
    names = list(Enum.GetNames(et))
    if value_name not in names:
        print(f"[WARN] {et.FullName} nemá hodnotu '{value_name}'. Dostupné: {', '.join(names)}")
        return False
    val = Enum.Parse(et, value_name)
    obj.GetType().GetProperty(prop_name).SetValue(obj, val, None)
    print(f"[OK] {prop_name} = {value_name} ({et.FullName})")
    return True

def set_bool_if_exists(obj, prop_name, value: bool):
    if obj is None: return False
    p = obj.GetType().GetProperty(prop_name)
    if p is None or p.PropertyType.FullName != "System.Boolean":
        return False
    p.SetValue(obj, value, None)
    print(f"[OK] {prop_name} = {value}")
    return True

# ---------- 4) Argumenty ----------
parser = argparse.ArgumentParser(description="Create TIA Portal project (V19, pythonnet 3.x)")
parser.add_argument("--dir", required=True, help="Target directory for project")
parser.add_argument("--name", required=True, help="Project name")
parser.add_argument("--type-id", required=True, help='PLC typeId e.g. "CPU_1212C_DC_DC_DC"')
parser.add_argument("--plc-version", default="V4.5", help='PLC version (default "V4.5")')
parser.add_argument("--plc-name", default="PLC_1", help='Logical device name (default "PLC_1")')
parser.add_argument("--ui", action="store_true", help="Run TIA with UI")
args = parser.parse_args()

# ---------- 5) TIA start + projekt ----------
from Siemens.Engineering import TiaPortal, TiaPortalMode
from Siemens.Engineering import DataType
from Siemens.Engineering.SW import PlcBlockType, PlcProgrammingLanguage

mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface
tia = TiaPortal(mode)
print(f"[INFO] TIA running (UI={args.ui}).")

Path(args.dir).mkdir(parents=True, exist_ok=True)
print(f"[INFO] Creating project '{args.name}' in '{args.dir}'")
project = tia.Projects.Create(args.dir, args.name)
devices = project.Devices

# ---------- 6) Přidej PLC a najdi CPU item ----------
import clr as _clr
deviceItemRef = _clr.Reference[object]()
device = devices.CreateWithItem(args.type_id, args.plc_version, args.plc_name, deviceItemRef)
root_item = deviceItemRef.Value

# najdi child s SoftwareContainer (CPU)
cpu_item = None
for it in root_item.DeviceItems:
    try:
        if it.GetService("SoftwareContainer") is not None:
            cpu_item = it
            break
    except:
        pass
if cpu_item is None:
    raise RuntimeError("CPU item with SoftwareContainer not found.")

# ---------- 7) Protection + StartInfo přes reflexi ----------
prot = cpu_item.GetService("Protection")
if prot:
    # Protection.Level (enum), DownloadWithoutRewire (bool)
    set_enum_by_name(prot, "Level", "FullAccess")
    set_bool_if_exists(prot, "DownloadWithoutRewire", True)
else:
    print("[WARN] Protection service not available.")

start_info = cpu_item.GetService("StartInfo")
if start_info:
    # StartInfo.StartMode (enum)
    set_enum_by_name(start_info, "StartMode", "AlwaysRun")
else:
    print("[WARN] StartInfo service not available.")

# ---------- 8) PLC software + ukázkový DB ----------
swc = cpu_item.GetService("SoftwareContainer")
if swc is None:
    raise RuntimeError("SoftwareContainer not found on CPU item.")
plc_sw = swc.Software

db = plc_sw.BlockGroup.Blocks.Create(PlcBlockType.DataBlock, "MyDataBlock", PlcProgrammingLanguage.LAD)
db.Interface.Static.Create("myRealVar", DataType.Real)

# ---------- 9) Ulož ----------
project.Save()
print(f"[SUCCESS] Project '{args.name}' created at '{args.dir}'.")
