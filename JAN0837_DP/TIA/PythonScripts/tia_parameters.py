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
    ("btnWestCrosswalk1", "Bool", "FALSE"),
    ("btnWestCrosswalk2", "Bool", "FALSE"),
    ("btnSouthCrosswalk1", "Bool", "FALSE"),
    ("btnSouthCrosswalk2", "Bool", "FALSE"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

Crossroad_Output_UDT_NAME = "Crossroad_Output"

Crossroad_Output_UDT_FIELDS = [
    ("crossroadType", "Bool", "FALSE"),
    ("trafficLightNorth_green", "Bool", "FALSE"),
    ("trafficLightNorth_yellow", "Bool", "FALSE"),
    ("trafficLightNorth_red", "Bool", "FALSE"),
    ("trafficLightSouth_green", "Bool", "FALSE"),
    ("trafficLightSouth_yellow", "Bool", "FALSE"),
    ("trafficLightSouth_red", "Bool", "FALSE"),
    ("trafficLightWest_green", "Bool", "FALSE"),
    ("trafficLightWest_yellow", "Bool", "FALSE"),
    ("trafficLightWest_red", "Bool", "FALSE"),
    ("trafficLightEast_green", "Bool", "FALSE"),
    ("trafficLightEast_yellow", "Bool", "FALSE"),
    ("trafficLightEast_red", "Bool", "FALSE"),
    ("pedestrianSouth1_green", "Bool", "FALSE"),
    ("pedestrianSouth1_red", "Bool", "FALSE"),
    ("pedestrianSouth2_green", "Bool", "FALSE"),
    ("pedestrianSouth2_red", "Bool", "FALSE"),
    ("pedestrianWest1_green", "Bool", "FALSE"),
    ("pedestrianWest1_red", "Bool", "FALSE"),
    ("pedestrianWest2_green", "Bool", "FALSE"),
    ("pedestrianWest2_red", "Bool", "FALSE"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

Crosswalk_Input_UDT_NAME = "Crosswalk_Input"

Crosswalk_Input_UDT_FIELDS = [
    ("btnStart", "Bool", "FALSE"),
    ("btnPause", "Bool", "FALSE"),
    ("btnStop", "Bool", "FALSE"),
    ("btnCrosswalk1", "Bool", "FALSE"),
    ("btnCrosswalk2", "Bool", "FALSE"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
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
    ("pedestrianLightRed2", "Bool", "FALSE"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

Regulator_Input_UDT_NAME = "Regulator_Input"

Regulator_Input_UDT_FIELDS = [   
    ("btnReset", "Bool", "FALSE"),
    ("switchstate", "Bool", "FALSE"),
    ("order", "Int", "1"),
    ("R1", "Real", "0.0"),
    ("R2", "Real", "0.0"),
    ("C1", "Real", "0.0"),
    ("C2", "Real", "0.0"),    
    ("Uc1", "Real", "0.0"),
    ("Uc2", "Real", "0.0"),
    ("Td", "Real", "0.0"),
    ("Ts", "Real", "0.0"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

Regulator_Output_UDT_NAME = "Regulator_Output"

Regulator_Output_UDT_FIELDS = [

    ("Uin", "Real", "0.0"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

CarLight_Input_UDT_NAME = "CarLight_Input"

CarLight_Input_UDT_FIELDS = [
    ("btnReset", "Bool", "FALSE"),
    ("error", "Bool", "FALSE"),
    ("sensorLight", "Bool", "FALSE"),
    ("sensorConnectorConnected", "Bool", "FALSE"),
    ("lowBeamLight", "Bool", "FALSE"),
    ("highBeamLight", "Bool", "FALSE"),
    ("turnLight", "Bool", "FALSE"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

CarLight_Output_UDT_NAME = "CarLight_Output"

CarLight_Output_UDT_FIELDS = [
    
    ("result", "Bool", "FALSE"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
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
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
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
    ("TimeDoorMovement", "Int", "0"),
    ("MEMDoor", "Bool", "FALSE"),
    ("MEMDoorMovement", "Bool", "FALSE"),
    ("MEMDoorClosingTrig", "Bool", "FALSE"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

WashingMachine_Input_UDT_NAME = "WashingMachine_Input"

WashingMachine_Input_UDT_FIELDS = [
    ("btnEmergencyStop", "Bool", "FALSE"),
    ("btnMachine", "Bool", "FALSE"),
    ("btnStop", "Bool", "FALSE"),
    ("ErrorSystem", "Bool", "FALSE"),
    ("Mode", "Int", "0"),
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
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
    ("spare1", "DWord", "0"),
    ("spare2", "DWord", "0"),
    ("spare3", "DWord", "0"),
    ("spare4", "DWord", "0")
]

DEFAULT_DB_NAME = "DB_ProcessData"
DEFAULT_DB_OPTIMIZED = True

# Optional: example device defaults (not enforced; CLI usually provides these)
# You can reference these as fallbacks where appropriate.
DEFAULT_PLC_NAME = "PLC1"
DEFAULT_TYPE_ID = "OrderNumber:6ES7 212-1AE40-0XB0/V4.6"

