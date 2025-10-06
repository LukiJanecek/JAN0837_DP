export const STATE_SCHEMA = {
  
  // test
  number: { type: 'int',    default: 0 },
  text:   { type: 'string', default: '' },
  toggle: { type: 'boolean', default: false },
  
  // Crossroad  
  btnCrossroadStart: { type: 'boolean', default: false },
  btnCrossroadPause: { type: 'boolean', default: false },
  btnCrossroadStop: { type: 'boolean', default: false },
  
  // basic variables 
  communicationRefreshInterval: { type: 'int', default: 50 },
  communicationActive: { type: 'boolean', default: false },
  visualizationActive: { type: 'boolean', default: false },
  
  // Crossroad variables
  crossroadType : { type: 'boolean', default: false },  // 0 - night, everything else - day
  btnCrosswalk1: { type: 'boolean', default: false },
  btnCrosswalk2: { type: 'boolean', default: false },
  traffic_light1_green: { type: 'boolean', default: false },
  traffic_light1_yellow: { type: 'boolean', default: false },
  traffic_light1_red: { type: 'boolean', default: false },
  traffic_light2_green: { type: 'boolean', default: false },
  traffic_light2_yellow: { type: 'boolean', default: false },
  traffic_light2_red: { type: 'boolean', default: false },
  pedestrian_light1_green: { type: 'boolean', default: false },
  pedestrian_light1_red: { type: 'boolean', default: false },
  pedestrian_light2_green: { type: 'boolean', default: false },
  pedestrian_light2_red: { type: 'boolean', default: false },

  // 
  '*': { type: 'any' }
};