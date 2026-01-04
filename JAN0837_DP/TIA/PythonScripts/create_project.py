## this is cool code but need improvement 

# create_project_full.py — TIA V19 + pythonnet 3.x, vše v jednom

import os, sys, clr
from pathlib import Path
from System.IO import DirectoryInfo
import argparse
from collections import deque

# ===== 1) Import/loader BLOK (POUŽÍVEJ V KAŽDÉM SKRIPTU) =====
TIA_ROOT = r"C:\Program Files\Siemens\Automation\Portal V19"
TIA_PUBLIC_V19 = os.path.join(TIA_ROOT, "PublicAPI", "V19")
TIA_BIN = os.path.join(TIA_ROOT, "Bin")

if hasattr(os, "add_dll_directory"):
    for d in (TIA_PUBLIC_V19, TIA_BIN):
        if os.path.isdir(d): os.add_dll_directory(d)
for d in (TIA_PUBLIC_V19, TIA_BIN):
    if os.path.isdir(d): sys.path.append(d)

import System
from System.Reflection import Assembly, AssemblyName, ReflectionTypeLoadException
from System import ResolveEventHandler, Enum

_SEARCH_ROOTS = [Path(TIA_PUBLIC_V19), Path(TIA_BIN), Path(TIA_ROOT)]
_CACHE = {}
def _resolve(sender, args):
    name = AssemblyName(args.Name).Name
    p = _CACHE.get(name)
    if p and p.is_file(): return Assembly.LoadFrom(str(p))
    for root in _SEARCH_ROOTS[:-1]:
        cand = root / f"{name}.dll"
        if cand.is_file():
            _CACHE[name] = cand
            return Assembly.LoadFrom(str(cand))
    for cand in _SEARCH_ROOTS[-1].rglob(f"{name}.dll"):
        _CACHE[name] = cand
        return Assembly.LoadFrom(str(cand))
    return None
_resolver_delegate = ResolveEventHandler(_resolve)
System.AppDomain.CurrentDomain.add_AssemblyResolve(_resolver_delegate)

clr.AddReference("Siemens.Engineering")
from Siemens.Engineering import TiaPortal, TiaPortalMode

# ===== 2) Pomocné funkce (odtud dál můžeš jen kopírovat) =====
asm = Assembly.Load("Siemens.Engineering")
def _all_types(a):
    try: return list(a.GetTypes())
    except ReflectionTypeLoadException as e:
        return [t for t in e.Types if t is not None]
_TYPES = _all_types(asm)

def find_enum(simple_name: str):
    for t in _TYPES:
        if t is not None and t.IsEnum and t.Name == simple_name:
            return t
    return None

def enum_val(enum_type, name: str):
    return Enum.Parse(enum_type, name)

def find_cpu_item(root_item):
    q = [root_item]
    while q:
        it = q.pop(0)
        try:
            if it.GetService("SoftwareContainer") is not None:
                return it
        except: pass
        try:
            for ch in it.DeviceItems: q.append(ch)
        except: pass
    return None

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

def get_or_start_tia(with_ui: bool) -> TiaPortal:
    """
    - Pokud TIA už běží, připojí se k první nalezené instanci.
    - Jinak spustí novou (s UI podle with_ui).
    """
    try:
        procs = list(TiaPortal.GetProcesses())  # běžící TIA procesy
    except Exception:
        procs = []

    if procs:
        # připojit k existující instanci
        proc = procs[0]
        try:
            # v některých verzích stačí TiaPortal(proc)
            return TiaPortal(proc)
        except TypeError:
            # fallback overload (některé buildy vyžadují bool)
            return TiaPortal(proc, True)

    # žádná neběží → spustit novou
    mode = TiaPortalMode.WithUserInterface if with_ui else TiaPortalMode.WithoutUserInterface
    return TiaPortal(mode)

def walk_items_and_find_software_container(device):
    """
    Projde celý strom device.DeviceItems (BFS).
    Vrací tuple (cpu_item, software_container) nebo (None, None).
    Současně sbírá diagnostické řádky pro případný výpis.
    """
    diag_lines = []
    from collections import deque
    q = deque()
    # začneme všemi top-level itemy
    try:
        for it in device.DeviceItems:
            q.append((it, 0))
    except Exception:
        pass

    while q:
        it, depth = q.popleft()
        name = ""
        try:
            name = it.Name
        except Exception:
            name = "<unnamed>"

        # zkusíme typické služby
        has_sc = False
        try:
            sc = it.GetService("SoftwareContainer")
            has_sc = sc is not None
        except Exception:
            sc = None

        try:
            prot = it.GetService("Protection")
        except Exception:
            prot = None

        try:
            si = it.GetService("StartInfo")
        except Exception:
            si = None

        # logovací řádek
        diag_lines.append(
            f"{'  '*depth}- {name}  "
            f"[SC={'Y' if has_sc else 'n'}, Prot={'Y' if prot else 'n'}, StartInfo={'Y' if si else 'n'}]"
        )

        if has_sc:
            return it, sc, diag_lines  # hotovo

        # rozšíříme o děti
        try:
            for ch in it.DeviceItems:
                q.append((ch, depth+1))
        except Exception:
            pass

    return None, None, diag_lines

def list_services(obj):
    names = []
    try:
        svc_prop = obj.GetType().GetProperty("Services")
        if svc_prop:
            col = svc_prop.GetValue(obj, None)
            for s in col:
                try:
                    names.append(getattr(s, "Name", str(s)))
                except:
                    pass
    except:
        pass
    return names

def try_get_any_sw_container(obj):
    """Zkus známé varianty služeb na zadaném objektu (device nebo deviceItem)."""
    for candidate in ("SoftwareContainer", "PlcSoftware", "Software"):
        try:
            svc = obj.GetService(candidate)
            if svc is not None:
                # Pokud jsme dostali rovnou PlcSoftware, normalizujme na objekt se .Software
                # tzn. vytvoříme wrapper s property Software
                if hasattr(svc, "Software"):
                    return ("SC", svc)  # standardní SoftwareContainer
                # některé buildy vrátí přímo SW instanci
                if svc.GetType().Name.endswith("PlcSoftware"):
                    class _Wrap: pass
                    w = _Wrap()
                    setattr(w, "Software", svc)
                    return ("SW", w)
        except:
            pass
    return (None, None)

def list_services_on(obj):
    names = []
    try:
        for s in obj.Services:
            try:
                names.append(getattr(s, "Name", s.GetType().Name))
            except:
                pass
    except:
        pass
    return names

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

# ===== 3) Argumenty =====
parser = argparse.ArgumentParser(description="Create TIA V19 project and add DB")
parser.add_argument("--dir", required=False, default=r"C:\TIAProjects", help="Parent folder for project")
parser.add_argument("--name", required=False, default="Test", help="Project name (folder will be <name>.ap19)")
parser.add_argument("--type-id", required=False, default="CPU_1212C_DC_DC_DC", help="PLC typeId")
parser.add_argument("--plc-version", default="V4.5", help="PLC version (default V4.5)")
parser.add_argument("--plc-name", default="PLC_1", help="Device logical name")
parser.add_argument("--ui", action="store_true", help="Run TIA with UI")
args = parser.parse_args()

# ===== 4) Vytvoření projektu + PLC + DB =====
mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface
tia = TiaPortal(mode)
#tia = get_or_start_tia(args.ui)
print(f"[INFO] TIA attached={bool(list(TiaPortal.GetProcesses()))} (UI={args.ui})")

Path(args.dir).mkdir(parents=True, exist_ok=True) # exist root folder 
parent_dir = DirectoryInfo(args.dir)
project_name = args.name
proj_folder = Path(args.dir) / f"{project_name}.ap19"
if proj_folder.exists():
    project = tia.Projects.Open(DirectoryInfo(str(proj_folder)))
    print(f"[OK] Project opened: {proj_folder}")
else:
    project = tia.Projects.Create(parent_dir, project_name)
    print(f"[OK] Project created: {project_name} in {args.dir}")
print(f"[NOTE] Ve File Exploreru hledej složku: {os.path.join(args.dir, args.name)}.ap19")

#import clr as _clr
#deviceItemRef = _clr.Reference[object]()
#device = project.Devices.CreateWithItem(args.type_id, args.plc_version, args.plc_name, deviceItemRef)
#root_item = deviceItemRef.Value
#print("[OK] PLC device added.")

#dc = project.Devices.GetType()
#for m in dc.GetMethods():
#    if m.Name == "CreateWithItem":
#        sig = ", ".join(p.ParameterType.FullName for p in m.GetParameters())
#        print("CreateWithItem(", sig, ")")

device = project.Devices.CreateWithItem(
    args.type_id, args.plc_name, args.plc_name
)
print("[OK] PLC device added.")

print("[INFO] Služby na zařízení:", list_services_on(device))

cpu_item, plc_sw = find_plc_software(device)
if plc_sw is None:
    raise RuntimeError("Nenalezl jsem PLC software ani po pokusu o vytvoření. " +
                       "Ověř v TIA, že zařízení podporuje SW a je nainstalován příslušný katalog.")

print(f"[OK] Našel jsem PLC software na: {getattr(cpu_item, 'Name', '<device>')}  ({plc_sw.GetType().FullName})")

# Protection
prot = cpu_item.GetService("Protection")
if prot:
    set_enum_prop(prot, "Level", "FullAccess")
    set_bool_prop(prot, "DownloadWithoutRewire", True)
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

# PLC Software + DB
swc = cpu_item.GetService("SoftwareContainer")
if swc is None: raise RuntimeError("SoftwareContainer not found on CPU item.")
plc_sw = swc.Software

PlcBlockType = find_enum("PlcBlockType")
PlcProgrammingLanguage = find_enum("PlcProgrammingLanguage")
if PlcBlockType is None or PlcProgrammingLanguage is None:
    raise RuntimeError("Cannot resolve PlcBlockType or PlcProgrammingLanguage.")

db = plc_sw.BlockGroup.Blocks.Create(
    enum_val(PlcBlockType, "DataBlock"),
    "MyDataBlock",
    enum_val(PlcProgrammingLanguage, "LAD")
)
if create_var_with_datatype(db.Interface.Static, "myRealVar", "Real"):
    print("[OK] DB variable myRealVar: REAL created.")
else:
    print("[WARN] Could not add DB variable (no suitable Create(string, DataType) overload).")

project.Save()
print("[SUCCESS] Project saved.")
