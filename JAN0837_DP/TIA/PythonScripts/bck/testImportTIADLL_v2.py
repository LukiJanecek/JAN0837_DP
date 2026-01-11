#!/usr/bin/env python3
import sys
import platform
import traceback
from pathlib import Path

print("Python executable:", sys.executable)
print("Python version:", sys.version.replace('\\n',' '))

# Ensure pythonnet (clr) available and give actionable error
try:
    import clr
except ModuleNotFoundError as ex:
    print("ERROR: pythonnet (clr) not found in this interpreter.", file=sys.stderr)
    print("Install it into the same python used by the app:", file=sys.stderr)
    print(f'  "{sys.executable}" -m pip install pythonnet', file=sys.stderr)
    sys.exit(10)
except Exception:
    print("ERROR while importing clr:", file=sys.stderr)
    traceback.print_exc(file=sys.stderr)
    sys.exit(11)

# now try import System (should work if pythonnet is correct)
try:
    import System
    print("System import OK.")
except Exception:
    print("ERROR: import System failed after clr import.", file=sys.stderr)
    traceback.print_exc(file=sys.stderr)
    sys.exit(12)

def try_import(name):
    try:
        __import__(name)
        return True, None
    except Exception as ex:
        return False, traceback.format_exc()

def main():
    print("Python executable:", sys.executable)
    print("Python version:", sys.version.replace("\n"," "))
    print("Platform:", platform.platform())
    print("Architecture:", platform.architecture())
    try:
        import struct
        print("Pointer size (bits):", struct.calcsize("P") * 8)
    except Exception:
        pass

    ok, err = try_import("clr")
    print("\nclr import:", "OK" if ok else "FAIL")
    if not ok:
        print(err)

    ok2, err2 = try_import("System")
    print("\nSystem import:", "OK" if ok2 else "FAIL")
    if not ok2:
        print(err2)

    # Show installed pythonnet version if available
    try:
        import pkg_resources
        d = [p for p in pkg_resources.working_set if p.key.lower().startswith("pythonnet")]
        if d:
            for p in d:
                print("\nInstalled package:", p.project_name, p.version)
        else:
            print("\npythonnet not installed (no matching package found).")
    except Exception:
        pass

    # Final advice if failure
    if not ok:
        print("\nIf clr import failed, install pythonnet into this interpreter:")
        print(f'"{sys.executable}" -m pip install pythonnet')
        return 1

    # Try small CLR usage (safe test)
    try:
        import clr
        import System
        print("\nCLR and System are available. System.Type: ", System.Type if hasattr(System, "Type") else "Type not found")
    except Exception:
        print("\nFailed to use CLR/System:")
        traceback.print_exc()
        return 2

    return 0

if __name__ == "__main__":
    rc = main()
    sys.exit(rc)