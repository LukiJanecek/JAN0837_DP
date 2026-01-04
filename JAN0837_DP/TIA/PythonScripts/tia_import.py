## this is functional code -> import.py is better

# tia_import.py — připraví Siemens Openness prostředí (V19 + pythonnet 3.x)
import os, sys, clr
from pathlib import Path

TIA_ROOT = r"C:\Program Files\Siemens\Automation\Portal V19"
TIA_PUBLIC_V19 = os.path.join(TIA_ROOT, "PublicAPI", "V19")
TIA_BIN = os.path.join(TIA_ROOT, "Bin")

if hasattr(os, "add_dll_directory"):
    for d in (TIA_PUBLIC_V19, TIA_BIN):
        if os.path.isdir(d): os.add_dll_directory(d)
for d in (TIA_PUBLIC_V19, TIA_BIN):
    if os.path.isdir(d): sys.path.append(d)

import System
from System.Reflection import Assembly, AssemblyName
from System import ResolveEventHandler

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

clr.AddReference("Siemens.Engineering")  # odteď můžeš importovat Siemens.Engineering
print("tia_import: Siemens.Engineering reference loaded")

## this is functional code 
