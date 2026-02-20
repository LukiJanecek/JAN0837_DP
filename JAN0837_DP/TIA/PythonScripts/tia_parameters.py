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

CarWash_Input_UDT_NAME = "CarWash_Input"

CarWash_Input_UDT_FIELDS = []

CarWash_Output_UDT_NAME = "CarWash_Output"

CarWash_Output_UDT_FIELDS = []

WashingMachine_Input_UDT_NAME = "WashingMachine_Input"

WashingMachine_Input_UDT_FIELDS = []

WashingMachine_Output_UDT_NAME = "WashingMachine_Output"

WashingMachine_Output_UDT_FIELDS = []

Regulator_Input_UDT_NAME = "Regulator_Input"

Regulator_Input_UDT_FIELDS = []

Regulator_Output_UDT_NAME = "Regulator_Output"

Regulator_Output_UDT_FIELDS = []

DEFAULT_DB_NAME = "DB_ProcessData"
DEFAULT_DB_OPTIMIZED = True

# Optional: example device defaults (not enforced; CLI usually provides these)
# You can reference these as fallbacks where appropriate.
DEFAULT_PLC_NAME = "PLC1"
DEFAULT_TYPE_ID = "OrderNumber:6ES7 212-1AE40-0XB0/V4.6"

