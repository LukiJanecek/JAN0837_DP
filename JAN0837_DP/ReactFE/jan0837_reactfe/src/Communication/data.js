export const STATE_SCHEMA = {

  number: { type: 'int',    default: 0 },
  text:   { type: 'string', default: '' },
  toggle: { type: 'boolean', default: false },

  '*': { type: 'any' }
};