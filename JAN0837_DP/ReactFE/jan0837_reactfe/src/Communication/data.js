export const STATE_SCHEMA = {

  // ── Test ──────────────────────────────────────────────
  TestData: { type: 'object', shape: {
    number: { type: 'int',     default: 0 },
    text:   { type: 'string',  default: '' },
    toggle: { type: 'boolean', default: false },
  }},

  // ── Crossroad ────────────────────────────────────────
  CrossroadData: { type: 'object', shape: {
    crossroadType:       { type: 'boolean', default: false }, // night / day
    btnCrossroadStart:   { type: 'boolean', default: false },
    btnCrossroadPause:   { type: 'boolean', default: false },
    btnCrossroadStop:    { type: 'boolean', default: false },
    btnCrosswalk1:       { type: 'boolean', default: false },
    btnCrosswalk2:       { type: 'boolean', default: false },
    trafficLight1_green: { type: 'boolean', default: false },
    trafficLight1_yellow:{ type: 'boolean', default: false },
    trafficLight1_red:   { type: 'boolean', default: false },
    trafficLight2_green: { type: 'boolean', default: false },
    trafficLight2_yellow:{ type: 'boolean', default: false },
    trafficLight2_red:   { type: 'boolean', default: false },
    pedestrian1_green:   { type: 'boolean', default: false },
    pedestrian1_red:     { type: 'boolean', default: false },
    pedestrian2_green:   { type: 'boolean', default: false },
    pedestrian2_red:     { type: 'boolean', default: false },
  }},

  // ── Crosswalk ────────────────────────────────────────
  CrosswalkData: { type: 'object', shape: {
    crosswalkType:                  { type: 'boolean', default: false }, // night / day
    btnCrosswalkStart:              { type: 'boolean', default: false },
    btnCrosswalkPause:              { type: 'boolean', default: false },
    btnCrosswalkStop:               { type: 'boolean', default: false },
    btnCrosswalk1_crosswalk:        { type: 'boolean', default: false },
    btnCrosswalk2_crosswalk:        { type: 'boolean', default: false },
    trafficLight1_green_crosswalk:  { type: 'boolean', default: false },
    trafficLight1_yellow_crosswalk: { type: 'boolean', default: false },
    trafficLight1_red_crosswalk:    { type: 'boolean', default: false },
    trafficLight2_green_crosswalk:  { type: 'boolean', default: false },
    trafficLight2_yellow_crosswalk: { type: 'boolean', default: false },
    trafficLight2_red_crosswalk:    { type: 'boolean', default: false },
    pedestrian1_green_crosswalk:    { type: 'boolean', default: false },
    pedestrian1_red_crosswalk:      { type: 'boolean', default: false },
    pedestrian2_green_crosswalk:    { type: 'boolean', default: false },
    pedestrian2_red_crosswalk:      { type: 'boolean', default: false },
  }},

  // ── Regulator ────────────────────────────────────────
  RegulatorData: { type: 'object', shape: {
    btnStart_regulator: { type: 'boolean', default: false },
    R: { type: 'int', default: 0 },
    C: { type: 'int', default: 0 },
    U: { type: 'int', default: 0 },
    I: { type: 'int', default: 0 },
  }},

  // ── CarWash ──────────────────────────────────────────
  CarWash: { type: 'object', shape: {
    btnCarWashEmergencyStop:  { type: 'boolean', default: false },
    btnStartCarWash:          { type: 'boolean', default: false },
    btnStopCarWash:           { type: 'boolean', default: false },
    CarWashErrorSystem:       { type: 'boolean', default: false },
    CarWashCarPosition:       { type: 'boolean', default: false },
    CarWashShowerPosition:    { type: 'boolean', default: false },
    CarWashMode:              { type: 'string',  default: '' },
    CarWashLight_green:       { type: 'boolean', default: false },
    CarWashLight_yellow:      { type: 'boolean', default: false },
    CarWashLight_red:         { type: 'boolean', default: false },
    CarWashDoor1_Up:          { type: 'boolean', default: false },
    CarWashDoor1_Down:        { type: 'boolean', default: false },
    CarWashDoor2_Up:          { type: 'boolean', default: false },
    CarWashDoor2_Down:        { type: 'boolean', default: false },
    CarWashChemicalsFront:    { type: 'boolean', default: false },
    CarWashChemicalsSides:    { type: 'boolean', default: false },
    CarWashChemicalsBack:     { type: 'boolean', default: false },
    CarWashPrewash:           { type: 'boolean', default: false },
    CarWashWater:             { type: 'boolean', default: false },
    CarWashWax:               { type: 'boolean', default: false },
    CarWashDry:               { type: 'boolean', default: false },
    CarWashBrushes:           { type: 'boolean', default: false },
    CarWashSoap:              { type: 'boolean', default: false },
    CarWashActiveFoam:        { type: 'boolean', default: false },
    CarWashTimeDoorMovement:  { type: 'string',  default: '' },
    CarWashMEMDoor:           { type: 'boolean', default: false },
    CarWashMEMDoorTrig:       { type: 'boolean', default: false },
    CarWashMEMDoorClosingtrig:{ type: 'boolean', default: false },
  }},

  // ── WashingMachine ───────────────────────────────────
  WashingMachine: { type: 'object', shape: {
    btnWashingMachineEmergencyStop: { type: 'boolean', default: false },
    btnStartWashingMachine:         { type: 'boolean', default: false },
    btnStopWashingMachine:          { type: 'boolean', default: false },
    WashingMachineErrorSystem:      { type: 'boolean', default: false },
    WashingMachineMode:             { type: 'string',  default: '' },
    // outputs
    WashingMachineLight_green:      { type: 'boolean', default: false },
    WashingMachineLight_yellow:     { type: 'boolean', default: false },
    WashingMachineLight_red:        { type: 'boolean', default: false },
    WashingMachineDoorClosed:       { type: 'boolean', default: false },
    WashingMachineChemicals:        { type: 'boolean', default: false },
    WashingMachinePrewash:          { type: 'boolean', default: false },
    WashingMachineWater:            { type: 'boolean', default: false },
    WashingMachineDry:              { type: 'boolean', default: false },
    WashingMachineBrushes:          { type: 'boolean', default: false },
    WashingMachineSoap:             { type: 'boolean', default: false },
    WashingMachineActiveFoam:       { type: 'boolean', default: false },
  }},

  // catch-all for unknown sections
  '*': { type: 'any' }
};