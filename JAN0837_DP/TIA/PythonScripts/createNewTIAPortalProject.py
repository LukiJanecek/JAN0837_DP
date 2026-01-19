#!/usr/bin/env python3
# to project on path add Data type and from data type create DB

# terminal test: python createNewTIAPortalProject.py --dll-dir "C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19" --project-dir "C:\Users\lukas\VSB-TUO\JAN0837 - Bakalářská práce - General\DP\JAN0837_DP\JAN0837_DP\TIA\TIA_projects\Sample\JAN0837_sample" --project-name "JAN0837_sample" --type-id "OrderNumber:6ES7 212-1AE40-0XB0/V4.6" --plc-name "PLC1" --ui

import sys
import argparse
import importTIADLL

def main():
    parser = argparse.ArgumentParser(description="Create new TIA Portal project with data type and DB")
    parser.add_argument("--dll-dir", required=True, help="Directory containing TIA DLLs")
    parser.add_argument("--project-dir", required=True, help="Project directory path")
    parser.add_argument("--project-name", required=True, help="Project name")
    parser.add_argument("--type-id", required=True, help="PLC type ID")
    parser.add_argument("--plc-name", required=True, help="PLC name")
    parser.add_argument("--ui", action="store_true", help="Open project with TIA Portal UI")
    
    args = parser.parse_args()
    
    # Task 1: import Siemens.Engineering.dll
    print("=" * 60)
    print("Task 1: Importing TIA DLL from", args.dll_dir)
    print("=" * 60)
    sys.argv = ["importTIADLL.py", "--dir", args.dll_dir]
    importTIADLL.main()

    from System.IO import FileInfo
    from Siemens.Engineering import TiaPortal, TiaPortalMode
    
    # Task 2: create new TIA Portal project on path with given name
    print("\n" + "=" * 60)
    print("Task 2: Creating new TIA Portal project:", args.project_name)
    print("=" * 60)
    # TODO: Implement project creation logic
    
    # Task 3: in project create new PLC with given type id and name
    print("\n" + "=" * 60)
    print("Task 3: Creating PLC:", args.plc_name, "with type:", args.type_id)
    print("=" * 60)
    # TODO: Implement PLC creation logic
    
    # Task 4: save project
    print("\n" + "=" * 60)
    print("Task 4: Saving project")
    print("=" * 60)
    # TODO: Implement project save logic
    
    # Task 5: add data type to PLC
    print("\n" + "=" * 60)
    print("Task 5: Adding data type to PLC")
    print("=" * 60)
    # TODO: Implement data type addition logic
    
    # Task 6: create DB in PLC with data type
    print("\n" + "=" * 60)
    print("Task 6: Creating DB in PLC with data type")
    print("=" * 60)
    # TODO: Implement DB creation logic
    
    # Task 7: open project with TIA Portal UI
    if args.ui:
        print("\n" + "=" * 60)
        print("Task 7: Opening project with TIA Portal UI")
        print("=" * 60)
        # TODO: Implement UI opening logic

if __name__ == "__main__":
    main()


