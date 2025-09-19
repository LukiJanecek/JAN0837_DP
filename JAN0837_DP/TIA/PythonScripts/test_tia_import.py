import os, sys
import clr

TIA_DLL_PATH = r"C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19"

if hasattr(os, "add_dll_directory"):
    os.add_dll_directory(TIA_DLL_PATH)
sys.path.append(TIA_DLL_PATH)

clr.AddReference("Siemens.Engineering")

from Siemens.Engineering import TiaPortal, TiaPortalMode
from Siemens.Engineering.HW import StartMode, ProtectionLevel
from Siemens.Engineering.SW import PlcBlockType, PlcProgrammingLanguage
from Siemens.Engineering import DataType

print("OK: V19 API načteno z jediné DLL.")