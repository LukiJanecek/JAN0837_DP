#!/usr/bin/env python3
# to project on path add Data type and from data type create DB
# terminal test: python addDBtoPathProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample" --ui

import sys
import argparse
import importTIADLL

def main():
    parser = argparse.ArgumentParser(description="Add DB to TIA Portal project")
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

    from System.IO import FileInfo
    from Siemens.Engineering import TiaPortal, TiaPortalMode

    # create data type in PLC 

    # Crossroad - input, output

    # crate DB in PLC with data type


if __name__ == "__main__":
    main()

