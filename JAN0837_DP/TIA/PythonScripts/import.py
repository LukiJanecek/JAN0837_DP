# --- TIA V19 + pythonnet 3.x: minimální a funkční import ---
import os, sys, clr
from pathlib import Path

# 1) Cesty k TIA V19 (pokud máš jinde, uprav TIA_ROOT)
TIA_ROOT = r"C:\Program Files\Siemens\Automation\Portal V19"
TIA_PUBLIC_V19 = os.path.join(TIA_ROOT, "PublicAPI", "V19")
TIA_BIN = os.path.join(TIA_ROOT, "Bin")

# 2) Připrav loader (nejdřív clr, pak System)
if hasattr(os, "add_dll_directory"):
    for d in (TIA_PUBLIC_V19, TIA_BIN):
        if os.path.isdir(d):
            os.add_dll_directory(d)
for d in (TIA_PUBLIC_V19, TIA_BIN):
    if os.path.isdir(d):
        sys.path.append(d)

import System
from System.Reflection import Assembly, AssemblyName
from System import ResolveEventHandler

# 3) Resolver: dohledá chybějící závislosti v PublicAPI\V19, Bin a celé instalaci
_SEARCH_ROOTS = [Path(TIA_PUBLIC_V19), Path(TIA_BIN), Path(TIA_ROOT)]
_CACHE = {}  # name -> Path

def _resolve(sender, args):
    name = AssemblyName(args.Name).Name  # např. "Siemens.Engineering.Contract"
    # cache
    p = _CACHE.get(name)
    if p and p.is_file():
        return Assembly.LoadFrom(str(p))
    # přímý pokus v PublicAPI\V19 a Bin
    for root in _SEARCH_ROOTS[:-1]:
        cand = root / f"{name}.dll"
        if cand.is_file():
            _CACHE[name] = cand
            return Assembly.LoadFrom(str(cand))
    # fallback: jednorázové dohledání v celé instalaci
    for cand in _SEARCH_ROOTS[-1].rglob(f"{name}.dll"):
        _CACHE[name] = cand
        return Assembly.LoadFrom(str(cand))
    return None

_resolver_delegate = ResolveEventHandler(_resolve)  # držet referenci!
System.AppDomain.CurrentDomain.add_AssemblyResolve(_resolver_delegate)

# 4) Načti hlavní assembly a otestuj import
clr.AddReference("Siemens.Engineering")
from Siemens.Engineering import TiaPortal, TiaPortalMode

print("✅ Siemens.Engineering načteno, TIA API připraveno.")
