export const STATE_SCHEMA = {

  // Test
  /*
  TestData : { type: 'object', shape: {
    number : { type: 'int', default: 0 },
    text : { type: 'string', default: '' },
    toggle : { type: 'boolean', default: false },
  }},
  */
 
  // Crossroad 
  CrossroadData: { type: 'object', shape: {
    // inputs
    btnStart : { type: 'boolean', default: false },
    btnPause : { type: 'boolean', default: false },
    btnStop : { type: 'boolean', default: false },
    btnWestCrosswalk1 : { type: 'boolean', default: false },
    btnWestCrosswalk2 : { type: 'boolean', default: false },
    btnSouthCrosswalk1 : { type: 'boolean', default: false },
    btnSouthCrosswalk2 : { type: 'boolean', default: false },
    // outputs
    crossroadType : { type: 'boolean', default: false }, // night / day
    trafficLightNorth_green : { type: 'boolean', default: false },
    trafficLightNorth_yellow : { type: 'boolean', default: false },
    trafficLightNorth_red : { type: 'boolean', default: false },
    trafficLightSouth_green : { type: 'boolean', default: false },
    trafficLightSouth_yellow : { type: 'boolean', default: false },
    trafficLightSouth_red : { type: 'boolean', default: false },
    trafficLightWest_green : { type: 'boolean', default: false },
    trafficLightWest_yellow : { type: 'boolean', default: false },
    trafficLightWest_red : { type: 'boolean', default: false },
    trafficLightEast_green : { type: 'boolean', default: false },
    trafficLightEast_yellow : { type: 'boolean', default: false },
    trafficLightEast_red : { type: 'boolean', default: false },
    pedestrianSouth1_green : { type: 'boolean', default: false },
    pedestrianSouth1_red : { type: 'boolean', default: false },
    pedestrianSouth2_green : { type: 'boolean', default: false },
    pedestrianSouth2_red : { type: 'boolean', default: false },
    pedestrianWest1_green : { type: 'boolean', default: false },
    pedestrianWest1_red : { type: 'boolean', default: false },
    pedestrianWest2_green : { type: 'boolean', default: false },
    pedestrianWest2_red : { type: 'boolean', default: false },
    //pedestrianNorth_green : { type: 'boolean', default: false },
    //pedestrianNorth_red : { type: 'boolean', default: false },
    //pedestrianEast_green : { type: 'boolean', default: false },
    //pedestrianEast_red : { type: 'boolean', default: false },
  }},

  // Crosswalk 
  CrosswalkData: { type: 'object', shape: {
    // inputs
    btnStart : { type: 'boolean', default: false },
    btnPause: { type: 'boolean', default: false },
    btnStop : { type: 'boolean', default: false },
    btnCrosswalk1 : { type: 'boolean', default: false },
    btnCrosswalk2 : { type: 'boolean', default: false },
    // outputs
    crosswalkType : { type: 'boolean', default: false }, // night / day
    trafficLight1_green : { type: 'boolean', default: false },
    trafficLight1_yellow : { type: 'boolean', default: false },
    trafficLight1_red : { type: 'boolean', default: false },
    trafficLight2_green : { type: 'boolean', default: false },
    trafficLight2_yellow : { type: 'boolean', default: false },
    trafficLight2_red : { type: 'boolean', default: false },
    pedestrian1_green : { type: 'boolean', default: false },
    pedestrian1_red : { type: 'boolean', default: false },
    pedestrian2_green : { type: 'boolean', default: false },
    pedestrian2_red : { type: 'boolean', default: false },
  }},

  // Regulator
  RegulatorData: { type: 'object', shape: {
    // inputs
    btnReset : { type: 'boolean', default: false },
    switchstate : { type: 'boolean', default: false },
    order : { type: 'integer', default: 1 },
    R1 : { type: 'real', default: 0.0 },
    R2 : { type: 'real', default: 0.0 },
    C1 : { type: 'real', default: 0.0 },
    C2 : { type: 'real', default: 0.0 },
    Uc1 : { type: 'real', default: 0.0 },
    Uc2 : { type: 'real', default: 0.0 },
    Td : { type: 'real', default: 0.0 }, // transport delay
    Ts : { type: 'real', default: 0.0 }, // sampling time
    // outputs
    Uin : { type: 'real', default: 0.0 },
  }},

  // CarLight 
  CarLight: { type: 'object', shape: {
    // inputs
    btnReset : { type: 'boolean', default: false },
    error : { type: 'boolean', default: false },
    sensorLight : { type: 'boolean', default: false },
    sensorConnectorConnected : { type: 'boolean', default: false },
    lowBeamLight : { type: 'boolean', default: false }, 
    highBeamLight : { type: 'boolean', default: false }, 
    turnLight : { type: 'boolean', default: false }, 
    // outputs
    result : { type: 'boolean', default: false },
  }},

  // CarWash 
  /*
  CarWash: { type: 'object', shape: {
    // inputs
    btnEmergencyStop : { type: 'boolean', default: false },
    btnStart : { type: 'boolean', default: false },
    btnStop : { type: 'boolean', default: false },
    ErrorSystem : { type: 'boolean', default: false },
    CarPosition : { type: 'boolean', default: false },
    ShowerPosition : { type: 'boolean', default: false },
    Mode : { type: 'integer',  default: 0 },
    // outputs 
    Light_green : { type: 'boolean', default: false },
    Light_yellow : { type: 'boolean', default: false },
    Light_red : { type: 'boolean', default: false },
    Door1_Up : { type: 'boolean', default: false },
    Door1_Down : { type: 'boolean', default: false },
    Door2_Up : { type: 'boolean', default: false },
    Door2_Down : { type: 'boolean', default: false },
    ChemicalsFront : { type: 'boolean', default: false },
    ChemicalsSides : { type: 'boolean', default: false },
    ChemicalsBack : { type: 'boolean', default: false },
    Prewash : { type: 'boolean', default: false },
    Water : { type: 'boolean', default: false },
    Wax : { type: 'boolean', default: false },
    Dry : { type: 'boolean', default: false },
    Brushes : { type: 'boolean', default: false },
    Soap : { type: 'boolean', default: false },
    ActiveFoam : { type: 'boolean', default: false },
    TimeDoorMovement : { type: 'integer',  default: 0 },
    MEMDoor : { type: 'boolean', default: false },
    MEMDoorTrig : { type: 'boolean', default: false },
    MEMDoorClosingtrig : { type: 'boolean', default: false },
  }},
  */

  // WashingMachine
  /*
  WashingMachine: { type: 'object', shape: {
    // inputs 
    btnEmergencyStop : { type: 'boolean', default: false },
    btnStart : { type: 'boolean', default: false },
    btnStop : { type: 'boolean', default: false },
    ErrorSystem : { type: 'boolean', default: false },
    Mode : { type: 'integer',  default: 0 },
    // outputs
    Light_green : { type: 'boolean', default: false },
    Light_yellow : { type: 'boolean', default: false },
    Light_red : { type: 'boolean', default: false },
    DoorClosed : { type: 'boolean', default: false },
    Chemicals : { type: 'boolean', default: false },
    Prewash : { type: 'boolean', default: false },
    Water : { type: 'boolean', default: false },
    Dry : { type: 'boolean', default: false },
    Brushes : { type: 'boolean', default: false },
    Soap : { type: 'boolean', default: false },
    ActiveFoam : { type: 'boolean', default: false },
  }},
  */

  // catch-all for unknown sections
  '*': { type: 'any' }
};