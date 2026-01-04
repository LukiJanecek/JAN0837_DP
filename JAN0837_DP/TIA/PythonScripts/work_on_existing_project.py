# this code doesnt work properly

# work_on_existing_project.py — připoj se k TIA, otevři existující projekt, přidej DB

import os, sys, clr
from pathlib import Path

# --- loader (klidně nahraď za 'import tia_import' pokud už ho máš) ---
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

# --- attach / start TIA ---
from Siemens.Engineering import TiaPortal, TiaPortalMode
def get_or_start_tia(with_ui: bool) -> TiaPortal:
    try:
        procs = list(TiaPortal.GetProcesses())
    except Exception:
        procs = []
    if procs:
        p = procs[0]
        if hasattr(TiaPortal, "Attach"):
            try: return TiaPortal.Attach(p)
            except: pass
        try: return TiaPortal(p)  # ctor(TiaPortalProcess)
        except: pass
    mode = TiaPortalMode.WithUserInterface if with_ui else TiaPortalMode.WithoutUserInterface
    return TiaPortal(mode)

# --- argy ---
import argparse
from System.IO import DirectoryInfo
parser = argparse.ArgumentParser(description="Open existing TIA project and add DB")
parser.add_argument("--open", required=True, help=r"Cesta ke složce projektu *.ap19, např. C:\TIAProjects\Template1212.ap19")
parser.add_argument("--ui", action="store_true", help="Použít TIA s UI")
args = parser.parse_args()

tia = get_or_start_tia(args.ui)
print("[INFO] Připojeno k TIA.")

# --- otevři projekt ---
proj_dir = args.open
if proj_dir.lower().endswith(".ap19") is False:
    raise SystemExit("Parametr --open musí být cesta ke složce *.ap19 (projekt).")
if not os.path.isdir(proj_dir):
    raise SystemExit(f"Složka projektu neexistuje: {proj_dir}")

project = tia.Projects.Open(DirectoryInfo(proj_dir))
print(f"[OK] Otevřen projekt: {proj_dir}")

# --- helpery pro enumy a nastavení ---
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

# --- najdi PLC software v otevřeném projektu ---
from collections import deque
def find_any_plc_software(prj):
    for dev in prj.Devices:
        # 1) root device
        try:
            sc = dev.GetService("SoftwareContainer")
            if sc is not None and getattr(sc, "Software", None) is not None:
                return dev, None, sc.Software
        except: pass
        # 2) projdi strom itemů
        q = deque()
        try:
            for it in dev.DeviceItems: q.append(it)
        except: pass
        while q:
            it = q.popleft()
            try:
                sc = it.GetService("SoftwareContainer")
                if sc is not None and getattr(sc, "Software", None) is not None:
                    return dev, it, sc.Software
            except: pass
            try:
                for svc in it.Services:
                    p = svc.GetType().GetProperty("Software")
                    if p:
                        sw = p.GetValue(svc, None)
                        if sw is not None and sw.GetType().Name.endswith("PlcSoftware"):
                            return dev, it, sw
            except: pass
            try:
                for ch in it.DeviceItems: q.append(ch)
            except: pass
    return None, None, None

device, cpu_item, plc_sw = find_any_plc_software(project)
if plc_sw is None:
    raise SystemExit("V otevřeném projektu jsem nenašel PLC software. Otevři projekt, kde už je 'Program blocks' apod.")

print(f"[OK] Nalezen PLC SW na zařízení '{getattr(device,'Name','<device>')}'. CPU uzel: {getattr(cpu_item,'Name','<device-root>')}")

# --- Protection = NoProtection (když existuje), StartMode volitelně ---
try:
    target_obj = (cpu_item or device)
    prot = target_obj.GetService("Protection")
except:
    prot = None

def enum_names_of_prop(obj, prop):
    try:
        p = obj.GetType().GetProperty(prop)
        et = p.PropertyType
        if et.IsEnum:
            from System import Enum as _Enum
            return list(_Enum.GetNames(et))
    except: pass
    return []

if prot:
    opts = enum_names_of_prop(prot, "Level")
    target = "NoProtection" if "NoProtection" in opts else ("FullAccess" if "FullAccess" in opts else None)
    if target: set_enum_prop(prot, "Level", target)
    set_bool_prop(prot, "DownloadWithoutRewire", True)
    print(f"[OK] Protection nastaven (Level={target or 'bez změny'}).")
else:
    print("[INFO] Protection služba není k dispozici (u S7-1200 je to OK).")

try:
    start_info = (cpu_item or device).GetService("StartInfo")
    if start_info and "AlwaysRun" in enum_names_of_prop(start_info, "StartMode"):
        set_enum_prop(start_info, "StartMode", "AlwaysRun")
        print("[OK] Start mode nastaven na AlwaysRun.")
except: pass

# --- vytvoř DB + proměnnou REAL ---
PlcBlockType = find_enum("PlcBlockType")
PlcProgrammingLanguage = find_enum("PlcProgrammingLanguage")
if PlcBlockType is None or PlcProgrammingLanguage is None:
    raise SystemExit("Nejde načíst PlcBlockType/PlcProgrammingLanguage.")

db = plc_sw.BlockGroup.Blocks.Create(
    enum_val(PlcBlockType, "DataBlock"),
    "MyDataBlock",
    enum_val(PlcProgrammingLanguage, "LAD")
)
ok = create_var_with_datatype(db.Interface.Static, "myRealVar", "Real")
print("[OK] DB vytvořen.", "Proměnná myRealVar: REAL přidána." if ok else "Proměnná se nepodařila přidat.")

project.Save()
print("[SUCCESS] Hotovo.")
