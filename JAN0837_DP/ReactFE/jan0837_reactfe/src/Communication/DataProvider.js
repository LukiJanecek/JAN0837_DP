import React, { createContext, useContext, useState, useEffect, useRef, useMemo } from 'react';

import { API_URL } from '../variables';
import { useRefresh } from './RefreshContext';
import { readState, writeState } from './service';
import { normalizeBySchema } from './dataTypeNormalizer';
import { STATE_SCHEMA } from './data';

const DataContext = createContext({
  data: null,
  error: null,
  isFetching: false,
  refresh: () => Promise.resolve(),
  saveData: () => Promise.resolve()
});

export function DataProvider({ children }) {
  const { interval } = useRefresh();
  const [data, setData] = useState();
  const [error, setError] = useState(null);
  const [isFetching, setIsFetching] = useState(false);

  const abortRef = useRef(null);
  const timerRef = useRef(null);

  const clearTimer = () => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
    } 

    timerRef.current = null;
  };

  const fetchOnce = async () => {
    setIsFetching(true);
    abortRef.current?.abort();
    const ctrl = new AbortController();
    abortRef.current = ctrl;

    try {
      const json = await readState(ctrl.signal);
      const normalized = normalizeBySchema(json || {}, STATE_SCHEMA);
      setData(normalized);
      setError(null);
    } 
    catch (e) {
      if (e.name !== 'AbortError') {
        setError(String(e.message || e));
      } 
    } 
    finally {
      setIsFetching(false);
    }
  };

  const scheduleNext = (ms) => {
    clearTimer();
    const delay = Number.isFinite(interval) && interval > 0 ? interval : (ms ?? 2000);
    timerRef.current = setTimeout(async () => {
      await fetchOnce();
      scheduleNext(); 
    }, delay);
  };

  useEffect(() => {
    fetchOnce().finally(() => scheduleNext());
    return () => {
      clearTimer();
      abortRef.current?.abort();
    };
  }, [interval]);

  const saveData = async (patch) => {
    await writeState(patch);
    await fetchOnce();
  };

  return (
    <DataContext.Provider value={{ data, error, isFetching, refresh: fetchOnce, saveData }}>
      {children}
    </DataContext.Provider>
  );
}

export const useData = () => useContext(DataContext);

/**
 * Hook for section-scoped data access.
 * Returns { section, saveSection } where:
 *   - section is the nested object for that section (e.g. data.CrossroadData)
 *   - saveSection(patch) wraps the patch into { [sectionName]: patch } before saving
 */
export function useSectionData(sectionName) {
  const { data, saveData, error, isFetching, refresh } = useData();

  const section = data?.[sectionName] ?? {};

  const saveSection = useMemo(
    () => async (patch) => {
      const payload = { [sectionName]: patch };
      console.log('[saveSection] Sending:', JSON.stringify(payload));
      const result = await saveData(payload);
      console.log('[saveSection] Result:', result);
      return result;
    },
    [saveData, sectionName]
  );

  return { section, saveSection, data, error, isFetching, refresh };
}
