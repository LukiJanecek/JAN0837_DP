#!/usr/bin/env python3
# to project on path add Data type and from data type create DB
# terminal test: python openPathProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample" --ui

import sys
import argparse
from pathlib import Path

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
    importTIADLL.main()

    # Now import System modules after DLL is loaded
    from System.IO import FileInfo
    from Siemens.Engineering import TiaPortal, TiaPortalMode

    # Task 2: open project on path
    # open project on path
    print("\n" + "=" * 60)
    print("Task 2: Opening TIA Portal project at", args.project_dir)
    print("=" * 60)
    
    # Determine which mode to use
    mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface
    
    # Open the TIA Portal
    tia_portal = TiaPortal(mode)    
    
    # Handle project path - TIA projects can be specified as:
    # 1. Path to folder: C:\...\JAN0837_sample
    # 2. Path to file: C:\...\JAN0837_sample\JAN0837_sample.ap19
    project_path = Path(args.project_dir)
    
    # If it's a folder, look for the .ap19 file inside it
    if project_path.is_dir():
        # Get the folder name and look for <folder_name>.ap19 inside
        folder_name = project_path.name
        ap19_file = project_path / f"{folder_name}.ap19"
        if ap19_file.exists():
            project_path = ap19_file
        else:
            # Try to find any .ap19 file inside
            ap19_files = list(project_path.glob("*.ap19"))
            if ap19_files:
                project_path = ap19_files[0]
    
    # Check if project file exists
    if not project_path.exists():
        print(f"[ERROR] Project file not found: {project_path}")
        # Debug: show what's in the folder
        parent = project_path.parent if project_path.is_file() else project_path
        print(f"[DEBUG] Contents of {parent}:")
        if parent.exists():
            for item in parent.iterdir():
                print(f"  - {item.name}")
        return
    
    # Debug: show what we're opening
    print(f"[DEBUG] Opening project file: {project_path}")
    
    # Open the project using FileInfo with the directory path
    project = tia_portal.Projects.Open(FileInfo(str(project_path)))
    
    print(f"[OK] Project opened successfully: {project_path}")
    print(f"Project name: {project.Name}")  

    # Close the project and portal
    if mode == TiaPortalMode.WithoutUserInterface:
        project.Close()
        tia_portal.Dispose()

if __name__ == "__main__":
    main()