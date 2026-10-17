import { createContext, useContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';
import { useApi } from '../api/api';
import type {Runner} from '../api/api.model';

interface RunnersContextValue {
  runners: Runner[];
  onlineCount: number;
  loaded: boolean;
}

const RunnersContext = createContext<RunnersContextValue>({
  runners: [],
  onlineCount: 0,
  loaded: false,
});

export const RunnersProvider = ({ children }: { children: ReactNode }) => {
  const api = useApi();
  const [runners, setRunners] = useState<Runner[]>([]);
  const [loaded, setLoaded] = useState(false);

  useEffect(() => {
    let mounted = true;
    const load = async () => {
      const data = await api.fetchRunners();
      if (mounted) {
        setRunners(data);
        setLoaded(true);
      }
    };
    load();
    const interval = setInterval(load, 5000);
    return () => {
      mounted = false;
      clearInterval(interval);
    };
  }, [api]);

  return (
    <RunnersContext.Provider value={{ runners, onlineCount: runners.filter(x => x.status === 'Online').length, loaded }}>
      {children}
    </RunnersContext.Provider>
  );
};

export const useRunners = () => useContext(RunnersContext);
