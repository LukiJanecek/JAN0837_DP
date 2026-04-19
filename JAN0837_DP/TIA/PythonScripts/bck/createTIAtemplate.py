## this doesnt work -> wrong add clr.AddReference("Siemens.Engineering.HW")

import clr
import sys
import os
import argparse

TIA_DLL_PATH = r"C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19"
sys.path.append(TIA_DLL_PATH)
clr.AddReference("Siemens.Engineering")
clr.AddReference("Siemens.Engineering.HW")
clr.AddReference("Siemens.Engineering.SW")
clr.AddReference("Siemens.Engineering.SW.Blocks")
clr.AddReference("Siemens.Engineering.SW.Types")

from Siemens.Engineering import TiaPortal, TiaPortalMode
from Siemens.Engineering.HW import StartMode, ProtectionLevel
from Siemens.Engineering.SW import PlcBlockType, PlcProgrammingLanguage
from Siemens.Engineering import DataType

parser = argparse.ArgumentParser(description="Create TIA Portal project")
parser.add_argument("--dir", required=True, help="Target directory for project")
parser.add_argument("--name", required=True, help="Project name")
parser.add_argument("--type-id", required=True, help="PLC type (e.g. CPU_1212C_DC_DC_DC)")
parser.add_argument("--ui", action="store_true", help="Start TIA with user interface")
args = parser.parse_args()

#tia = TiaPortal(TiaPortalMode.WithUserInterface)
#print("TIA Portal running")

mode = TiaPortalMode.WithUserInterface if args.ui else TiaPortalMode.WithoutUserInterface
tia = TiaPortal(mode)
print(f"TIA Portal running in mode: {mode}")

project_path = args.dir
project_name = args.name

project = tia.Projects.Create(project_path, project_name)

# Add CPU
devices = project.Devices
deviceItemRef = clr.Reference[object]()
device = devices.CreateWithItem(args.type_id, "V4.5", "PLC_1", deviceItemRef)
plc_device_item = deviceItemRef.Value

# Set protection
protection = plc_device_item.DeviceItems[1].GetService("Protection")
if protection:
    protection.Level = ProtectionLevel.FullAccess
    protection.DownloadWithoutRewire = True

# Set start mode
startInfo = plc_device_item.DeviceItems[1].GetService("StartInfo")
if startInfo:
    startInfo.StartMode = StartMode.AlwaysRun

# PLC SF
software_container = plc_device_item.DeviceItems[1].GetService("SoftwareContainer")
plc_software = software_container.Software

# Create DB
block_group = plc_software.BlockGroup
db = block_group.Blocks.Create(PlcBlockType.DataBlock, "MyDataBlock", PlcProgrammingLanguage.LAD)
static_section = db.Interface.Static
static_section.Create("myRealVar", DataType.Real)

# Save project
project.Save()
print("Project craeted and saved.")



