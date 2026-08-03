#!/usr/bin/env python3
# Open an existing TIA Portal project.
#
# EXAMPLES (use forward slashes - easier in PowerShell):
#
# With UI:
# python openPathProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample/JAN0837_sample" --ui
#
# Without UI (background mode):
# python openPathProject.py --dll-dir "C:/Program Files/Siemens/Automation/Portal V19/PublicAPI/V19" --project-dir "C:/Users/lukas/VSB-TUO/JAN0837 - Bakalářská práce - General/DP/JAN0837_DP/JAN0837_DP/TIA/TIA_projects/Sample/JAN0837_sample"

import sys
import argparse
from pathlib import Path

import tia_parameters as params
import tia_functions as fc
import importTIADLL

def main():
    parser = argparse.ArgumentParser(description="Open TIA Portal project")
    parser.add_argument("--dll-dir", required=True, help="Directory containing TIA DLLs")
    parser.add_argument("--project-dir", required=True, help="Project directory path")
    parser.add_argument("--ui", action="store_true", help="Open project with TIA Portal UI")
    
    args = parser.parse_args()
    
    # Task 1: import Siemens.Engineering.dll
    # import Siemens.Engineering.dll
    print("=" * 60)
    print("Task 1: Importing TIA DLL from", args.dll_dir)
    print("=" * 60)
    sys.argv = ["importTIADLL.py", "--dir", args.dll_dir]
    import_result = fc.import_tia_dll(args.dll_dir)
    if import_result != 0:
        return import_result

    # Now import System modules after DLL is loaded
    from Siemens.Engineering import TiaPortal, TiaPortalMode

    # Task 2: open project on path
    # open project on path
    print("\n" + "=" * 60)
    print("Task 2: Opening TIA Portal project at", args.project_dir)
    print("=" * 60)
    
    # Determine which mode to use
    mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface
    tia_portal = TiaPortal(mode)
    
    # Resolve project file
    try:
        project_path = fc.locate_project_file(args.project_dir)
    except FileNotFoundError as e:
        print(f"[ERROR] {e}")
        return 2
    
    # Debug: show what we're opening
    print(f"[DEBUG] Opening project file: {project_path}")
    
    # Open directly when versions match; upgrade the immediately previous version.
    project = fc.open_project(tia_portal, project_path)
    
    print(f"[OK] Project opened successfully: {project_path}")
    print(f"Project name: {project.Name}")  

    # Close if in background mode
    fc.close_if_headless(None, tia_portal, mode)
    if mode == TiaPortalMode.WithUserInterface:
        print("[NOTE] Project remains open in TIA Portal UI.")

    return 0

if __name__ == "__main__":
    rc = main()
    sys.exit(rc if isinstance(rc, int) else 1)
