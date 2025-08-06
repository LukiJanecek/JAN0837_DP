import { createContext, useContext, useState } from 'react';

import { API_URL } from './variables';
import { interval } from './variables';

const RefreshContext = createContext({
  interval,
  setInterval: () => {}
});

export function RefreshProvider({ children }) {
  const [interval, setIntervalValue] = useState(2000);
  return (
    <RefreshContext.Provider value={{ interval, setInterval: setIntervalValue }}>
      {children}
    </RefreshContext.Provider>
  );
}

export const useRefresh = () => useContext(RefreshContext);