#!/usr/bin/env python3
# with UI: python startTIAPortal.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --ui
# without UI: python startTIAPortal.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19"

import sys
import os
import argparse
from pathlib import Path

import tia_parameters as params
import tia_functions as fc
import importTIADLL

def main():
    parser = argparse.ArgumentParser(description="Start TIA Portal")
    parser.add_argument("--dll-dir", required=True, help="Directory containing TIA DLLs")
    parser.add_argument("--ui", action="store_true", help="Open project with TIA Portal UI")

    args = parser.parse_args()

    # Task 1: import Siemens.Engineering.dll
    print("=" * 60)
    print("Task 1: Importing TIA DLL from", args.dll_dir)
    print("=" * 60)
    sys.argv = ["importTIADLL.py", "--dir", args.dll_dir]
    fc.import_tia_dll(args.dll_dir)

    import System
    from System.Reflection import Assembly, AssemblyName, ReflectionTypeLoadException
    from System import ResolveEventHandler, Enum
    from System.IO import FileInfo, DirectoryInfo
    from Siemens.Engineering import TiaPortal, TiaPortalMode

    # Task 2: open TIA Portal
    print("\n" + "=" * 60)
    print("Task 2: Starting TIA Portal")
    print("=" * 60)
    mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface
    tia_portal = TiaPortal(mode)

    print("[OK] TIA Portal started in", "UI mode" if mode == TiaPortalMode.WithUserInterface else "background mode")

    # Close if in background mode
    fc.close_if_headless(None, tia_portal, mode)
    if mode == TiaPortalMode.WithUserInterface:
        print("[NOTE] Project remains open in TIA Portal UI.")

    return 0

if __name__ == "__main__":
    rc = main()
    sys.exit(rc if isinstance(rc, int) else 1)
