import { createContext, useContext, useState } from 'react';

const RefreshContext = createContext({
  interval: 2000,
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