export const STATE_SCHEMA = {
  
  // test
  number: { type: 'int',    default: 0 },
  text:   { type: 'string', default: '' },
  toggle: { type: 'boolean', default: false },
  
  // basic variables 
  communicationRefreshInterval: { type: 'int', default: 2000 },
  communicationActive: { type: 'boolean', default: false },
  visualizationActive: { type: 'boolean', default: false },

  // Crossroad  
  btnCrosswalk1: { type: 'boolean', default: false },
  btnCrosswalk2: { type: 'boolean', default: false },
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

  // 
  '*': { type: 'any' }
};