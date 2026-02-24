"""Centralized parameters and defaults for TIA scripts.

Keep only configuration-like values here. Code and Siemens API calls
belong in tia_functions.py.
"""

# Default names and options used by scripts
DEFAULT_UDT_NAME = "MyDataType"
DEFAULT_UDT_FIELDS = [
	("button", "Bool", "FALSE"),
	("LED", "Bool", "FALSE"),
]

Crossroad_Input_UDT_NAME = "Crossroad_Input"

Crossroad_Input_UDT_FIELDS = [
    ("btnStart", "Bool", "FALSE"),
    ("btnPause", "Bool", "FALSE"),
    ("btnStop", "Bool", "FALSE"),
    ("btnCrosswalk1", "Bool", "FALSE"),
    ("btnCrosswalk2", "Bool", "FALSE")
]

Crossroad_Output_UDT_NAME = "Crossroad_Output"

Crossroad_Output_UDT_FIELDS = [
    ("crossroadType", "Bool", "FALSE"),
    ("trafficLightGreen1", "Bool", "FALSE"),
    ("trafficLightsYellow1", "Bool", "FALSE"),
    ("trafficLightsRed1", "Bool", "FALSE"),
    ("trafficLightGreen2", "Bool", "FALSE"),
    ("trafficLightsYellow2", "Bool", "FALSE"),
    ("trafficLightsRed2", "Bool", "FALSE"),
    ("pedestrianLightGreen1", "Bool", "FALSE"),
    ("pedestrianLightRed1", "Bool", "FALSE"),
    ("pedestrianLightGreen2", "Bool", "FALSE"),
    ("pedestrianLightRed2", "Bool", "FALSE")
]

Crosswalk_Input_UDT_NAME = "Crosswalk_Input"

Crosswalk_Input_UDT_FIELDS = [
    ("btnStart", "Bool", "FALSE"),
    ("btnPause", "Bool", "FALSE"),
    ("btnStop", "Bool", "FALSE"),
    ("btnCrosswalk1", "Bool", "FALSE"),
    ("btnCrosswalk2", "Bool", "FALSE")
]

Crosswalk_Output_UDT_NAME = "Crosswalk_Output"

Crosswalk_Output_UDT_FIELDS = [
    ("crosswalkType", "Bool", "FALSE"),
    ("trafficLightGreen1", "Bool", "FALSE"),
    ("trafficLightsYellow1", "Bool", "FALSE"),
    ("trafficLightsRed1", "Bool", "FALSE"),
    ("trafficLightGreen2", "Bool", "FALSE"),
    ("trafficLightsYellow2", "Bool", "FALSE"),
    ("trafficLightsRed2", "Bool", "FALSE"),
    ("pedestrianLightGreen1", "Bool", "FALSE"),
    ("pedestrianLightRed1", "Bool", "FALSE"),
    ("pedestrianLightGreen2", "Bool", "FALSE"),
    ("pedestrianLightRed2", "Bool", "FALSE")
]

Regulator_Input_UDT_NAME = "Regulator_Input"

Regulator_Input_UDT_FIELDS = [   
    ("switchstate", "Bool", "FALSE"),
    ("R", "Real", "0.0"),
    ("C", "Real", "0.0"),
    ("U", "Real", "0.0"),
    ("Td", "Real", "0.0"),
]

Regulator_Output_UDT_NAME = "Regulator_Output"

Regulator_Output_UDT_FIELDS = [
    ("Uc", "Real", "0.0"),
]

CarWash_Input_UDT_NAME = "CarWash_Input"

CarWash_Input_UDT_FIELDS = [
    ("btnEmergencyStop", "Bool", "FALSE"),
    ("btnStart", "Bool", "FALSE"),
    ("btnStop", "Bool", "FALSE"),
    ("ErrorSystem", "Bool", "FALSE"),
    ("CarPosition", "Bool", "FALSE"),
    ("ShowerPosition", "Bool", "FALSE"),
    ("Mode", "Int", "0"),
]

CarWash_Output_UDT_NAME = "CarWash_Output"

CarWash_Output_UDT_FIELDS = [
    ("Light_green", "Bool", "FALSE"),
    ("Light_yellow", "Bool", "FALSE"),
    ("Light_red", "Bool", "FALSE"),
    ("Door1_Up", "Bool", "FALSE"),
    ("Door1_Down", "Bool", "FALSE"),
    ("Door2_Up", "Bool", "FALSE"),
    ("CarWashDoor2_Down", "Bool", "FALSE"),
    ("ChemicalsFront", "Bool", "FALSE"),
    ("ChemicalsSides", "Bool", "FALSE"),
    ("ChemicalsBack", "Bool", "FALSE"),
    ("Prewash", "Bool", "FALSE"),
    ("Water", "Bool", "FALSE"),
    ("Wax", "Bool", "FALSE"),
    ("Dry", "Bool", "FALSE"),
    ("Brushes", "Bool", "FALSE"),
    ("Soap", "Bool", "FALSE"),
    ("TimeDoorMovement", "Bool", "FALSE"),
    ("MEMDoor", "Bool", "FALSE"),
    ("MEMDoorMovement", "Bool", "FALSE"),
    ("MEMDoorClosingTrig", "Bool", "FALSE"),
]

WashingMachine_Input_UDT_NAME = "WashingMachine_Input"

WashingMachine_Input_UDT_FIELDS = [
    ("btnEmergencyStop", "Bool", "FALSE"),
    ("btnMachine", "Bool", "FALSE"),
    ("btnStop", "Bool", "FALSE"),
    ("ErrorSystem", "Bool", "FALSE"),
    ("Mode", "Int", "0"),
]

WashingMachine_Output_UDT_NAME = "WashingMachine_Output"

WashingMachine_Output_UDT_FIELDS = [
    ("Light_green", "Bool", "FALSE"),
    ("Light_yellow", "Bool", "FALSE"),
    ("Light_red", "Bool", "FALSE"),
    ("DoorClosed", "Bool", "FALSE"),
    ("PreWash", "Bool", "FALSE"),
    ("Water", "Bool", "FALSE"),
    ("Wax", "Bool", "FALSE"),
    ("Dry", "Bool", "FALSE"),
    ("Brushes", "Bool", "FALSE"),
    ("Soap", "Bool", "FALSE"),
    ("ActiveFoam", "Bool", "FALSE"),
]

CarLight_Input_UDT_NAME = "CarLight_Input"

CarLight_Input_UDT_FIELDS = [
    ("btnStart", "Bool", "FALSE"),
    ("btnReset", "Bool", "FALSE"),
    ("markerLight", "Bool", "FALSE"),
    ("brakeLight", "Bool", "FALSE"),
    ("turnLight", "Bool", "FALSE"),
]

CarLight_Output_UDT_NAME = "CarLight_Output"

CarLight_Output_UDT_FIELDS = [
    ("sensorPosition", "Bool", "FALSE"),
    ("sensorConnectorConnected", "Bool", "FALSE"),
    ("done", "Bool", "FALSE"),
]

DEFAULT_DB_NAME = "DB_ProcessData"
DEFAULT_DB_OPTIMIZED = True

# Optional: example device defaults (not enforced; CLI usually provides these)
# You can reference these as fallbacks where appropriate.
DEFAULT_PLC_NAME = "PLC1"
DEFAULT_TYPE_ID = "OrderNumber:6ES7 212-1AE40-0XB0/V4.6"

