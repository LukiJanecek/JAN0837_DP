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
    ("dopravnizpozdeni", "Real", "0.0"),
]

Regulator_Output_UDT_NAME = "Regulator_Output"

Regulator_Output_UDT_FIELDS = [
    ("Uc", "Real", "0.0"),
]

CarWash_Input_UDT_NAME = "CarWash_Input"

CarWash_Input_UDT_FIELDS = [
    ("btnCarWashEmergencyStop", "Bool", "FALSE"),
    ("btnStartCarWash", "Bool", "FALSE"),
    ("btnStopCarWash", "Bool", "FALSE"),
    ("CarWashErrorSystem", "Bool", "FALSE"),
    ("CarWashCarPosition", "Bool", "FALSE"),
    ("CarWashShowerPosition", "Bool", "FALSE"),
    ("CarWashMode", "String", "''"),
]

CarWash_Output_UDT_NAME = "CarWash_Output"

CarWash_Output_UDT_FIELDS = [
    ("CarWashLight_green", "Bool", "FALSE"),
    ("CarWashLight_yellow", "Bool", "FALSE"),
    ("CarWashLight_red", "Bool", "FALSE"),
    ("CarWashDoor1_Up", "Bool", "FALSE"),
    ("CarWashDoor1_Down", "Bool", "FALSE"),
    ("CarWashDoor2_Up", "Bool", "FALSE"),
    ("CarWashDoor2_Down", "Bool", "FALSE"),
    ("CarWashChemicalsFront", "Bool", "FALSE"),
    ("CarWashChemicalsSides", "Bool", "FALSE"),
    ("CarWashChemicalsBack", "Bool", "FALSE"),
    ("CarWashPrewash", "Bool", "FALSE"),
    ("CarWashWater", "Bool", "FALSE"),
    ("CarWashWax", "Bool", "FALSE"),
    ("CarWashDry", "Bool", "FALSE"),
    ("CarWashBrushes", "Bool", "FALSE"),
    ("CarWashSoap", "Bool", "FALSE"),
    ("CarWashTimeDoorMovement", "Bool", "FALSE"),
    ("CarWashMEMDoor", "Bool", "FALSE"),
    ("CarWashMEMDoorMovement", "Bool", "FALSE"),
    ("CarWashMEMDoorClosingTrig", "Bool", "FALSE"),
]

WashingMachine_Input_UDT_NAME = "WashingMachine_Input"

WashingMachine_Input_UDT_FIELDS = [
    ("btnWashingMachineEmergencyStop", "Bool", "FALSE"),
    ("btnStartWashingMachine", "Bool", "FALSE"),
    ("btnStopWashingMachine", "Bool", "FALSE"),
    ("WashingMachineErrorSystem", "Bool", "FALSE"),
    ("WashingMachineMode", "String", "''"),
]

WashingMachine_Output_UDT_NAME = "WashingMachine_Output"

WashingMachine_Output_UDT_FIELDS = [
    ("WashingMachineLight_green", "Bool", "FALSE"),
    ("WashingMachineLight_yellow", "Bool", "FALSE"),
    ("WashingMachineLight_red", "Bool", "FALSE"),
    ("WashingMachineDoorClosed", "Bool", "FALSE"),
    ("WashingMachinePreWash", "Bool", "FALSE"),
    ("WashingMachineWater", "Bool", "FALSE"),
    ("WashingMachineWax", "Bool", "FALSE"),
    ("WashingMachineDry", "Bool", "FALSE"),
    ("WashingMachineBrushes", "Bool", "FALSE"),
    ("WashingMachineSoap", "Bool", "FALSE"),
    ("WashingMachineActiveFoam", "Bool", "FALSE"),
]

CarLight_Input_UDT_NAME = "CarLight_Input"

CarLight_Input_UDT_FIELDS = [
    ("btnCarLightEmergencyStop", "Bool", "FALSE"),
    ("btnStartCarLight", "Bool", "FALSE"),
    ("btnStopCarLight", "Bool", "FALSE"),
    ("CarLightErrorSystem", "Bool", "FALSE"),
    ("CarLightMode", "String", "''"),
]

CarLight_Output_UDT_NAME = "CarLight_Output"

CarLight_Output_UDT_FIELDS = [
    ("CarLightLight_green", "Bool", "FALSE"),
    ("CarLightLight_yellow", "Bool", "FALSE"),
    ("CarLightLight_red", "Bool", "FALSE"),
    ("CarLightHeadlights", "Bool", "FALSE"),
    ("CarLightTaillights", "Bool", "FALSE"),
    ("CarLightTurnSignalLeft", "Bool", "FALSE"),
    ("CarLightTurnSignalRight", "Bool", "FALSE"),
    ("CarLightHazardLights", "Bool", "FALSE"),
]

DEFAULT_DB_NAME = "DB_ProcessData"
DEFAULT_DB_OPTIMIZED = True

# Optional: example device defaults (not enforced; CLI usually provides these)
# You can reference these as fallbacks where appropriate.
DEFAULT_PLC_NAME = "PLC1"
DEFAULT_TYPE_ID = "OrderNumber:6ES7 212-1AE40-0XB0/V4.6"

