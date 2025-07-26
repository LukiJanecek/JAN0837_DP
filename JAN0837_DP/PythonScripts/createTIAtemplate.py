import clr
import sys
import os

# Cesta ke knihovnám Siemens
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

tia = TiaPortal(TiaPortalMode.WithUserInterface)
print("TIA Portal spuštìno")

# Cesta k projektu
project_path = r"C:\TIAProjects\MyPythonProject"
project_name = "MyPythonProject"

# Vytvoø nový projekt
project = tia.Projects.Create(project_path, project_name)
devices = project.Devices

# Pøidej zaøízení: CPU 1212C DC/DC/DC v HW katalogu
deviceItemRef = clr.Reference[object]()
device = devices.CreateWithItem("CPU_1212C_DC_DC_DC", "V4.5", "PLC_1", deviceItemRef)
plc_device_item = deviceItemRef.Value

# Nastav zabezpeèení
protection = plc_device_item.DeviceItems[1].GetService("Protection")
if protection:
    protection.Level = ProtectionLevel.FullAccess
    protection.DownloadWithoutRewire = True

# Nastav start mode
startInfo = plc_device_item.DeviceItems[1].GetService("StartInfo")
if startInfo:
    startInfo.StartMode = StartMode.AlwaysRun

# Pøístup k PLC softwaru
software_container = plc_device_item.DeviceItems[1].GetService("SoftwareContainer")
plc_software = software_container.Software

# Vytvoø datový blok
block_group = plc_software.BlockGroup
db = block_group.Blocks.Create(PlcBlockType.DataBlock, "MyDataBlock", PlcProgrammingLanguage.LAD)
static_section = db.Interface.Static
static_section.Create("myRealVar", DataType.Real)

# Ulož projekt
project.Save()
print("Projekt vytvoøen a uložen")



