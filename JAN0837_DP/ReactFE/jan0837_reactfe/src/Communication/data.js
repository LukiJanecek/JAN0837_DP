export const STATE_SCHEMA = {
  
  // test
  number: { type: 'int',    default: 0 },
  text:   { type: 'string', default: '' },
  toggle: { type: 'boolean', default: false },
  
  // basic variables 
  //communicationRefreshInterval: { type: 'int', default: 50 },
  
  // Crossroad variables
  btnCrossroadStart: { type: 'boolean', default: false },
  btnCrossroadPause: { type: 'boolean', default: false },
  btnCrossroadStop: { type: 'boolean', default: false },
  btnCrosswalk1: { type: 'boolean', default: false },
  btnCrosswalk2: { type: 'boolean', default: false },
  crossroadType: { type: 'boolean', default: false },  // 'night' or 'day'
  trafficLight1_green: { type: 'boolean', default: false },
  trafficLight1_yellow: { type: 'boolean', default: false },
  trafficLight1_red: { type: 'boolean', default: false },
  trafficLight2_green: { type: 'boolean', default: false },
  trafficLight2_yellow: { type: 'boolean', default: false },
  trafficLight2_red: { type: 'boolean', default: false },
  pedestrian1_green: { type: 'boolean', default: false },
  pedestrian1_red: { type: 'boolean', default: false },
  pedestrian2_green: { type: 'boolean', default: false },
  pedestrian2_red: { type: 'boolean', default: false },

  // Crosswall variables 
  btnCrosswalkStart: { type: 'boolean', default: false },
  btnCrosswalkPause: { type: 'boolean', default: false },
  btnCrosswalkStop: { type: 'boolean', default: false },
  btnCrosswalk1: { type: 'boolean', default: false },
  btnCrosswalk2: { type: 'boolean', default: false },
  crosswalkType: { type: 'boolean', default: false },  // 'night' or 'day'
  trafficLight1_green: { type: 'boolean', default: false },
  trafficLight1_yellow: { type: 'boolean', default: false },
  trafficLight1_red: { type: 'boolean', default: false },
  trafficLight2_green: { type: 'boolean', default: false },
  trafficLight2_yellow: { type: 'boolean', default: false },
  trafficLight2_red: { type: 'boolean', default: false },
  pedestrian1_green: { type: 'boolean', default: false },
  pedestrian1_red: { type: 'boolean', default: false },
  pedestrian2_green: { type: 'boolean', default: false },
  pedestrian2_red: { type: 'boolean', default: false },

  // Regulator variables 
  btnStart: { type: 'boolean', default: false },
  R: { type: 'int', default: 0 },
  C: { type: 'int', default: 0 },
  U: { type: 'int', default: 0 },
  I: { type: 'int', default: 0 },

  // CarWash variables
  btnCarWashEmergencyStop: { type: 'boolean', default: false },
  btnStartCarWash: { type: 'boolean', default: false },
  btnStopCarWash: { type: 'boolean', default: false },
  CarWashErrorSystem: { type: 'boolean', default: false },
  CarWashCarPosition: { type: 'boolean', default: false },
  CarWashShowerPosition: { type: 'boolean', default: false },
  CarWashMode: { type: 'string', default: "" }, // int?
  CarWashLight_green: { type: 'boolean', default: false },
  CarWashLight_yellow: { type: 'boolean', default: false },
  CarWashLight_red: { type: 'boolean', default: false },
  CarWashDoor1_Up: { type: 'boolean', default: false },
  CarWashDoor1_Down: { type: 'boolean', default: false },
  CarWashDoor2_Up: { type: 'boolean', default: false },
  CarWashDoor2_Down: { type: 'boolean', default: false },
  CarWashChemicalsFront: { type: 'boolean', default: false },
  CarWashChemicalsSides: { type: 'boolean', default: false },
  CarWashChemicalsBack: { type: 'boolean', default: false },
  CarWashPrewash: { type: 'boolean', default: false },
  CarWashWater: { type: 'boolean', default: false },
  CarWashWax: { type: 'boolean', default: false },
  CarWashDry: { type: 'boolean', default: false },
  CarWashBrushes: { type: 'boolean', default: false },
  CarWashSoap: { type: 'boolean', default: false },
  CarWashActiveFoam: { type: 'boolean', default: false },
  CarWashTimeDoorMovement: { type: 'boolean', default: false }, // int? (time)
  CarWashMEMDoor: { type: 'boolean', default: false }, // bool
  CarWashMEMDoorTrig: { type: 'boolean', default: false }, // bool
  CarWashMEMDoorClosingtrig: { type: 'boolean', default: false }, // bool

  // WashingMachine variables
  btnWashingMachineEmergencyStop: { type: 'boolean', default: false },
  btnStartWashingMachine: { type: 'boolean', default: false },
  btnStopWashingMachine: { type: 'boolean', default: false },
  WashingMachineErrorSystem: { type: 'boolean', default: false },
  WashingMachineMode: { type: 'string', default: "" }, // int?

  //outputs
  WashingMachineLight_green: { type: 'boolean', default: false },
  WashingMachineLight_yellow: { type: 'boolean', default: false },
  WashingMachineLight_red: { type: 'boolean', default: false },
  WashingMachineDoorClosed: { type: 'boolean', default: false },
  WashingMachineChemicals: { type: 'boolean', default: false },
  WashingMachinePrewash: { type: 'boolean', default: false },
  WashingMachineWater: { type: 'boolean', default: false },
  WashingMachineWax: { type: 'boolean', default: false },
  WashingMachineDry: { type: 'boolean', default: false },
  WashingMachineBrushes: { type: 'boolean', default: false },
  WashingMachineSoap: { type: 'boolean', default: false },
  WashingMachineActiveFoam: { type: 'boolean', default: false },

  // 
  '*': { type: 'any' }
};