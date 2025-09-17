# find_datatype.py  — pythonnet 3.x, TIA V19
import os, sys, clr
from pathlib import Path

# --- Cesty k TIA V19 (podle tvého stroje) ---
TIA_ROOT = r"C:\Program Files\Siemens\Automation\Portal V19"
TIA_PUBLIC_V19 = os.path.join(TIA_ROOT, "PublicAPI", "V19")
TIA_BIN = os.path.join(TIA_ROOT, "Bin")

# Loaderu řekneme, kde hledat native/managed DLL
if hasattr(os, "add_dll_directory"):
    for d in (TIA_PUBLIC_V19, TIA_BIN):
        if os.path.isdir(d): os.add_dll_directory(d)
for d in (TIA_PUBLIC_V19, TIA_BIN):
    if os.path.isdir(d): sys.path.append(d)

# DŮLEŽITÉ: nejdřív clr, až pak System/Assembly
import System
from System.Reflection import Assembly, AssemblyName, ReflectionTypeLoadException
from System import ResolveEventHandler, Enum

# --- AssemblyResolve handler (pythonnet 3.x vyžaduje delegáta) ---
_SEARCH_ROOTS = [Path(TIA_PUBLIC_V19), Path(TIA_BIN), Path(TIA_ROOT)]
_CACHE = {}

def _find(name_no_ext: str) -> Path | None:
    p = _CACHE.get(name_no_ext)
    if p and p.is_file(): return p
    # rychlé pokusy
    for root in _SEARCH_ROOTS[:-1]:
        cand = root / f"{name_no_ext}.dll"
        if cand.is_file():
            _CACHE[name_no_ext] = cand
            return cand
    # fallback: rekurzivně v TIA_ROOT (jen jednou)
    for p in _SEARCH_ROOTS[-1].rglob(f"{name_no_ext}.dll"):
        _CACHE[name_no_ext] = p
        return p
    return None

def _resolve(sender, args):
    name = AssemblyName(args.Name).Name
    dll = _find(name)
    if dll:
        try:
            return Assembly.LoadFrom(str(dll))
        except Exception:
            return None
    return None

_resolver_delegate = ResolveEventHandler(_resolve)
System.AppDomain.CurrentDomain.add_AssemblyResolve(_resolver_delegate)

# --- Načti TIA assembly ---
clr.AddReference("Siemens.Engineering")
asm = Assembly.Load("Siemens.Engineering")

def _all_types(a):
    try:
        return list(a.GetTypes())
    except ReflectionTypeLoadException as e:
        return [t for t in e.Types if t is not None]

_TYPES = _all_types(asm)

# --- Najdi enum DataType (robustně) ---
def try_get_datatype_enum():
    # 1) klasické importy, pokud fungují
    try:
        from Siemens.Engineering import DataType as DT
        return DT, lambda name: getattr(DT, name)
    except ImportError:
        pass
    try:
        from Siemens.Engineering.SW.Types import DataType as DT
        return DT, lambda name: getattr(DT, name)
    except ImportError:
        pass
    # 2) fallback: reflexí přes všechny typy v assembly
    for t in _TYPES:
        if t is not None and t.IsEnum and t.Name == "DataType":
            return t, lambda name: Enum.Parse(t, name)
    return None, None

DataTypeEnum, DataTypeValue = try_get_datatype_enum()
if DataTypeEnum is None:
    print("❌ Enum 'DataType' nenalezen.")
else:
    print("✅ DataType typ:", DataTypeEnum.FullName)
    try:
        names = list(Enum.GetNames(DataTypeEnum))
        print("Dostupné hodnoty:", ", ".join(names))
        # příklad použití:
        print("Ukázka: DataTypeValue('Real') ->", DataTypeValue("Real"))
    except Exception:
        print("Pozn.: Tento DataType není enum (nebo nešel vypsat přes Enum.GetNames).")
