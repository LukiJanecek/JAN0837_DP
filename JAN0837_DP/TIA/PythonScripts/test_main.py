# probe_tia_v19.py  (pythonnet 3.x, robustní resolver)
import os, sys, clr
from pathlib import Path

# --- 1) Najdi TIA Portal V19 root (registry + fallback) ---
def find_tia_root():
    try:
        import winreg
        for keypath in [
            r"SOFTWARE\Siemens\Automation\TIAPortal\V19",
            r"SOFTWARE\WOW6432Node\Siemens\Automation\TIAPortal\V19",
        ]:
            for hive in (winreg.HKEY_LOCAL_MACHINE,):
                try:
                    with winreg.OpenKey(hive, keypath) as k:
                        val, _ = winreg.QueryValueEx(k, "InstallationPath")
                        if val and os.path.isdir(val):
                            return val
                except FileNotFoundError:
                    pass
    except Exception:
        pass
    # Fallback kandidáti
    for c in [
        r"C:\Program Files\Siemens\Automation\Portal V19",
        r"C:\Program Files\Siemens\Automation\Portal V19.0",
    ]:
        if os.path.isdir(c):
            return c
    raise RuntimeError("Nenalezl jsem TIA Portal V19 instalaci.")

TIA_ROOT = find_tia_root()
TIA_PUBLIC_V19 = os.path.join(TIA_ROOT, "PublicAPI", "V19")
TIA_BIN = os.path.join(TIA_ROOT, "Bin")

print(f"[INFO] TIA_ROOT = {TIA_ROOT}")
print(f"[INFO] TIA_PUBLIC_V19 = {TIA_PUBLIC_V19}")
print(f"[INFO] TIA_BIN = {TIA_BIN}")

# --- 2) Přidej složky do loaderu ---
if hasattr(os, "add_dll_directory"):
    for d in [TIA_PUBLIC_V19, TIA_BIN]:
        if os.path.isdir(d):
            os.add_dll_directory(d)
for d in [TIA_PUBLIC_V19, TIA_BIN]:
    if os.path.isdir(d):
        sys.path.append(d)

# --- 3) AssemblyResolve handler s cache a fallback rekurzí ---
import System
from System.Reflection import Assembly, AssemblyName, ReflectionTypeLoadException
from System import ResolveEventHandler

_SEARCH_ROOTS = [Path(TIA_PUBLIC_V19), Path(TIA_BIN), Path(TIA_ROOT)]
_CACHE = {}  # name -> Path

def _find_dll(name_no_ext: str) -> Path | None:
    # Cache hit?
    p = _CACHE.get(name_no_ext)
    if p and p.is_file():
        return p
    # Nejprve zkus přímo v kořenových složkách
    for root in _SEARCH_ROOTS[:-1]:
        cand = root / f"{name_no_ext}.dll"
        if cand.is_file():
            _CACHE[name_no_ext] = cand
            return cand
    # Fallback: rekurzivně v TIA_ROOT (může chvilku trvat, ale jednorázově)
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

# --- 4) Načti Siemens.Engineering a bezpečně projdi typy ---
clr.AddReference("Siemens.Engineering")
asm = Assembly.Load("Siemens.Engineering")

def _all_types(a):
    try:
        return list(a.GetTypes())
    except ReflectionTypeLoadException as e:
        # Nahlas chybějící závislosti (hinty)
        print("[WARN] LoaderExceptions:")
        for ex in e.LoaderExceptions:
            try:
                print(" -", ex.Message)
            except Exception:
                pass
        return [t for t in e.Types if t is not None]

types = _all_types(asm)

# --- 5) Najdi enumy bez ohledu na namespace ---
def resolve_enum(simple_name: str):
    for t in types:
        if t is not None and t.IsEnum and t.Name == simple_name:
            return t
    return None

protection_level_t = resolve_enum("ProtectionLevel")
start_mode_t = resolve_enum("StartMode")

print("Found types:")
print("  ProtectionLevel:", getattr(protection_level_t, "FullName", "NOT FOUND"))
print("  StartMode:", getattr(start_mode_t, "FullName", "NOT FOUND"))
