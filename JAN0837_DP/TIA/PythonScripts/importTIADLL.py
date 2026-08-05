#!/usr/bin/env python3
# Test loader for TIA PublicAPI folder
# terminal test: python importTIADLL.py --dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19"

import sys, os, argparse, traceback
from pathlib import Path

import tia_parameters as params
import tia_functions as fc

def eprint(*args, **kwargs):
    print(*args, file=sys.stderr, **kwargs)

def main():
    parser = argparse.ArgumentParser(description="Load TIA PublicAPI assemblies via pythonnet.")
    parser.add_argument("--dir", required=True, help="TIA PublicAPI folder (legacy or V21+)")
    args = parser.parse_args()

    selected_dir = Path(args.dir).expanduser().resolve()
    marker_names = ("Siemens.Engineering.Base.dll", "Siemens.Engineering.dll")
    dll_dir = selected_dir
    if selected_dir.is_dir() and not any((selected_dir / marker).is_file() for marker in marker_names):
        matches = [p.parent for marker in marker_names for p in selected_dir.rglob(marker)]
        if matches:
            dll_dir = sorted(set(matches), key=lambda p: ("Base" in " ".join(x.name for x in p.glob("*.dll")), str(p)), reverse=True)[0]
    print("Using DLL directory:", dll_dir)
    if not dll_dir.exists() or not dll_dir.is_dir():
        eprint(f"ERROR: directory does not exist: {dll_dir}")
        return 2

    # Ensure native DLL search path
    if hasattr(os, "add_dll_directory"):
        try:
            os.add_dll_directory(str(dll_dir))
        except Exception:
            pass

    # Add to sys.path so pythonnet can find managed assemblies by name
    if str(dll_dir) not in sys.path:
        sys.path.insert(0, str(dll_dir))

    try:
        import clr
    except ModuleNotFoundError:
        eprint("ERROR: pythonnet (clr) not installed in this interpreter.")
        eprint(f'Install with: "{sys.executable}" -m pip install pythonnet')
        return 10
    except Exception:
        eprint("ERROR: failed importing clr")
        traceback.print_exc(file=sys.stderr)
        return 11

    # Optional: assembly resolver to search the folder for dependencies
    try:
        import System
        from System.Reflection import Assembly, AssemblyName
        from System import ResolveEventHandler

        SEARCH_ROOTS = [dll_dir]
        CACHE = {}

        def _resolve(sender, args):
            name = AssemblyName(args.Name).Name
            p = CACHE.get(name)
            if p and p.is_file():
                return Assembly.LoadFrom(str(p))
            for root in SEARCH_ROOTS:
                cand = root / f"{name}.dll"
                if cand.is_file():
                    CACHE[name] = cand
                    return Assembly.LoadFrom(str(cand))
            # fallback: not found
            return None

        _resolver_delegate = ResolveEventHandler(_resolve)
        System.AppDomain.CurrentDomain.add_AssemblyResolve(_resolver_delegate)
    except Exception:
        eprint("WARNING: Could not register AssemblyResolve helper (continuing).")
        traceback.print_exc(file=sys.stderr)

    # V20 and older use Siemens.Engineering.dll. V21+ use modular Base/Step7 assemblies,
    # while keeping the public Python namespace Siemens.Engineering.
    try:
        legacy = dll_dir / "Siemens.Engineering.dll"
        base = dll_dir / "Siemens.Engineering.Base.dll"
        if legacy.is_file():
            clr.AddReference(str(legacy))
            api_kind = "legacy"
        elif base.is_file():
            clr.AddReference(str(base))
            for modular in sorted(dll_dir.glob("Siemens.Engineering.*.dll")):
                if modular.name.startswith("Siemens.Engineering.AddIn.") or modular == base:
                    continue
                try:
                    clr.AddReference(str(modular))
                except Exception as ex:
                    eprint(f"WARNING: optional module {modular.name} was not loaded: {ex}")
            api_kind = "modular V21+"
        else:
            eprint(f"ERROR: neither {' nor '.join(marker_names)} was found below {selected_dir}")
            return 4
        # import to validate
        import Siemens.Engineering as SE
        print(f"Siemens.Engineering import OK ({api_kind})")

        # show a few diagnostics
        try:
            tp = SE.TiaPortal
            print("TiaPortal type available:", tp)
        except Exception:
            pass

        # print assemblies found in the directory 
        dlls = sorted(dll_dir.glob("*.dll"))
        print(f"Found {len(dlls)} .dll files in directory (first 2000):")

        for p in dlls[:2000]:
            print("  " + str(p.name))

        return 0
    except Exception:
        eprint("ERROR: failed to load Siemens.Engineering via clr.AddReference.")
        traceback.print_exc(file=sys.stderr)
       
        try:
            dlls = sorted(dll_dir.glob("*.dll"))
            eprint(f"DLLs in folder ({len(dlls)}):")
            for p in dlls[:2000]:
                eprint("  " + str(p.name))
        except Exception:
            pass
        return 3

if __name__ == "__main__":
    rc = main()
    sys.exit(rc if isinstance(rc, int) else 1)
