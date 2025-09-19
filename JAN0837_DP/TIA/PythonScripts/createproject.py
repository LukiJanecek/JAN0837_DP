# create_or_clone_project.py — TIA V19 + pythonnet 3.x
import os, sys, clr, shutil
from pathlib import Path
import argparse

# --- loader (máš-li tia_import.py, nahraď prvních ~50 řádků za: import tia_import) ---
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
from System.IO import DirectoryInfo

def get_or_start_tia(with_ui: bool) -> TiaPortal:
    try:
        procs = list(TiaPortal.GetProcesses())
    except Exception:
        procs = []
    if procs:
        p = procs[0]
        # některé verze mají Attach
        if hasattr(TiaPortal, "Attach"):
            try: return TiaPortal.Attach(p)
            except: pass
        # většina buildů umí ctor(TiaPortalProcess)
        try: return TiaPortal(p)
        except: pass
    mode = TiaPortalMode.WithUserInterface if with_ui else TiaPortalMode.WithoutUserInterface
    return TiaPortal(mode)

# --- helpers (enumy, vlastnosti, hledání PLC SW) ---
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

from collections import deque
def find_plc_software_in_project(prj):
    for dev in prj.Devices:
        # root device
        try:
            sc = dev.GetService("SoftwareContainer")
            if sc is not None and getattr(sc, "Software", None) is not None:
                return prj, dev, None, sc.Software
        except: pass
        # device items
        q = deque()
        try:
            for it in dev.DeviceItems: q.append(it)
        except: pass
        while q:
            it = q.popleft()
            try:
                sc = it.GetService("SoftwareContainer")
                if sc is not None and getattr(sc, "Software", None) is not None:
                    return prj, dev, it, sc.Software
            except: pass
            try:
                for svc in it.Services:
                    p = svc.GetType().GetProperty("Software")
                    if p:
                        sw = p.GetValue(svc, None)
                        if sw is not None and sw.GetType().Name.endswith("PlcSoftware"):
                            return prj, dev, it, sw
            except: pass
            try:
                for ch in it.DeviceItems: q.append(ch)
            except: pass
    return None, None, None, None

# --- args ---
ap = argparse.ArgumentParser()
ap.add_argument("--dir", required=True, help=r"Parent folder (např. C:\TIAProjects)")
ap.add_argument("--name", required=True, help=r"Název projektu (vznikne složka <name>.ap19)")
ap.add_argument("--type-id", required=False, default="OrderNumber:6ES7 212-1AE40-0XB0/V4.6", help="TypeIdentifier zařízení (pokud zakládáš z nuly)")
ap.add_argument("--plc-name", default="PLC_1")
ap.add_argument("--ui", action="store_true")
ap.add_argument("--template", help=r"Cesta k *.ap19 složce (pokud chceš klonovat šablonu)")
args = ap.parse_args()

proj_parent = Path(args.dir)
proj_parent.mkdir(parents=True, exist_ok=True)
target_proj_dir = proj_parent / f"{args.name}.ap19"

tia = get_or_start_tia(args.ui)
print("[INFO] Připojeno k TIA.")

project = None

if args.template:
    tpl = Path(args.template)
    if not tpl.is_dir() or tpl.suffix.lower() != ".ap19":
        raise SystemExit(f"Šablona neexistuje nebo to není .ap19 složka: {tpl}")
    if target_proj_dir.exists():
        raise SystemExit(f"Cílový projekt už existuje: {target_proj_dir}")
    print(f"[INFO] Kopíruji šablonu: {tpl} -> {target_proj_dir}")
    shutil.copytree(tpl, target_proj_dir)
    project = tia.Projects.Open(DirectoryInfo(str(target_proj_dir)))
    print(f"[OK] Otevřen klon projektu: {target_proj_dir}")
else:
    # založ nový prázdný projekt
    project = tia.Projects.Create(DirectoryInfo(str(proj_parent)), args.name)
    print(f"[OK] Vytvořen nový projekt: {target_proj_dir}")
    # vlož zařízení (HW), POZOR: v tvé instalaci nemusí existovat SW služby → DB pak nepůjde přidat
    device = project.Devices.CreateWithItem(args.type_id, args.plc_name, args.plc_name)
    print("[OK] Vloženo zařízení:", args.type_id)

# --- najdi PLC SW v projektu (u šablony bude vždy, u nového jen pokud prostředí SW generuje) ---
_, device, cpu_item, plc_sw = find_plc_software_in_project(project)
if plc_sw is None:
    # jasná zpráva a návod
    raise SystemExit(
        "V projektu není PLC software (Program blocks/PLC tags). "
        "V téhle instalaci TIA/Openness nejde SW vytvořit API z nuly. "
        "Spusť skript s parametrem --template <cesta_k_sablone.ap19>."
    )

# --- Protection / StartMode (pokud existují služby) ---
try:
    target_obj = (cpu_item or device)
    prot = target_obj.GetService("Protection")
except: prot = None
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
print("[SUCCESS] Hotovo:", target_proj_dir)
