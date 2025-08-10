import React, { createContext, useContext, useState, useEffect } from 'react';

import { API_URL } from '../variables';
import { useRefresh } from './RefreshContext';

const DataContext = createContext({
  data: null,
  saveData: () => Promise.resolve()
});

export function DataProvider({ children }) {
  const [data, setData] = useState(null);
  const { interval } = useRefresh();

  // 1) Polling GET
  useEffect(() => {
    let isMounted = true;

    const fetchData = async () => {
      try {
        const res = await fetch(API_URL);
        if (!res.ok) throw new Error(res.statusText);
        const json = await res.json();
        if (isMounted) setData(json);
      } catch (err) {
        console.error("Chyba při načítání dat:", err);
      }
    };

    fetchData();
    const timer = setInterval(fetchData, interval);
    return () => {
      isMounted = false;
      clearInterval(timer);
    };
  }, [interval]);

  // 2) Funkce pro zápis
  const saveData = async (newValues) => {
    try {
      const res = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newValues)
      });
      if (!res.ok) throw new Error(res.statusText);
      // volitelně: refresh po zápisu
      return await res.json();
    } catch (err) {
      console.error("Chyba při ukládání dat:", err);
      throw err;
    }
  };

  return (
    <DataContext.Provider value={{ data, saveData }}>
      {children}
    </DataContext.Provider>
  );
}

export const useData = () => useContext(DataContext);
