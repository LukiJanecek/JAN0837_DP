#!/usr/bin/env python
# Minimal testImportTIADLL.py
# Usage: python testImportTIADLL.py --dir "C:\path\to\dll\folder"

import os
import sys
import argparse
import traceback
from pathlib import Path

# --- TIA V19 + pythonnet loader (user-provided preamble) ---
TIA_ROOT = r"C:\Program Files\Siemens\Automation\Portal V19"
TIA_PUBLIC_V19 = os.path.join(TIA_ROOT, "PublicAPI", "V19")
TIA_BIN = os.path.join(TIA_ROOT, "Bin")

# allow loading native DLLs on Python 3.8+ (adds to DLL search path)
if hasattr(os, "add_dll_directory"):
    for d in (TIA_PUBLIC_V19, TIA_BIN):
        if os.path.isdir(d):
            os.add_dll_directory(d)

# add PublicAPI and Bin to sys.path so pythonnet can find managed assemblies
for d in (TIA_PUBLIC_V19, TIA_BIN):
    if os.path.isdir(d) and d not in sys.path:
        sys.path.append(d)

import System
from System.Reflection import Assembly, AssemblyName
from System import ResolveEventHandler

_SEARCH_ROOTS = [Path(TIA_PUBLIC_V19), Path(TIA_BIN), Path(TIA_ROOT)]
_CACHE = {}  # name -> Path

def _resolve(sender, args):
    name = AssemblyName(args.Name).Name
    p = _CACHE.get(name)
    if p and p.is_file():
        return Assembly.LoadFrom(str(p))
    # check PublicAPI and Bin
    for root in _SEARCH_ROOTS[:-1]:
        cand = root / f"{name}.dll"
        if cand.is_file():
            _CACHE[name] = cand
            return Assembly.LoadFrom(str(cand))
    # fallback: search installation tree once
    for cand in _SEARCH_ROOTS[-1].rglob(f"{name}.dll"):
        _CACHE[name] = cand
        return Assembly.LoadFrom(str(cand))
    return None

_resolver_delegate = ResolveEventHandler(_resolve)
System.AppDomain.CurrentDomain.add_AssemblyResolve(_resolver_delegate)

import clr

# small helper to write to stderr
def eprint(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)

def main():
    parser = argparse.ArgumentParser(description="Test-import TIA DLL folder into pythonnet runtime.")
    parser.add_argument("--dir", required=True, help="Directory containing TIA-related .dll files")
    args = parser.parse_args()

    dll_dir = Path(args.dir).expanduser().resolve()
    if not dll_dir.exists() or not dll_dir.is_dir():
        eprint(f"ERROR: Directory does not exist: {dll_dir}")
        return 2

    # enumerate dlls
    dlls = sorted(dll_dir.glob("*.dll"), key=lambda p: p.name.lower())
    if not dlls:
        eprint(f"WARNING: No .dll files found in {dll_dir}")
    loaded = []
    failed = []

    for dll in dlls:
        try:
            # Try to load managed assembly by path
            Assembly.LoadFrom(str(dll))
            loaded.append(dll)
        except Exception as ex:
            # Not a managed assembly or failed to load - try clr.AddReferenceToFileAndPath as fallback
            try:
                clr.AddReferenceToFileAndPath(str(dll))
                loaded.append(dll)
            except Exception as ex2:
                failed.append((dll, str(ex2)))

    # Try to import Siemens.Engineering to verify TIA API availability
    try:
        clr.AddReference("Siemens.Engineering")
        from Siemens.Engineering import TiaPortal, TiaPortalMode  # test import
        print("Siemens.Engineering available.")
    except Exception as ex:
        eprint("ERROR: Siemens.Engineering not importable after loading DLLs.")
        eprint(traceback.format_exc())
        # still provide info about loaded/failed DLLs
        if loaded:
            print(f"Loaded {len(loaded)} assemblies (names):")
            for p in loaded[:20]:
                print("  " + str(p))
        if failed:
            eprint(f"Failed to load {len(failed)} assemblies:")
            for p, msg in failed[:20]:
                eprint(f"  {p}: {msg}")
        return 3

    # success summary
    print(f"Loaded {len(loaded)} assemblies from {dll_dir}")
    if failed:
        eprint(f"Failed loading {len(failed)} assemblies (non-fatal):")
        for p, msg in failed:
            eprint(f"  {p}: {msg}")

    print("TIA DLL import test finished successfully.")
    return 0

if __name__ == "__main__":
    try:
        rc = main()
        sys.exit(rc if isinstance(rc, int) else 0)
    except Exception:
        eprint("Unhandled exception:")
        eprint(traceback.format_exc())
        sys.exit(1)