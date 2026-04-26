import { createContext, useContext, useEffect, useState, useRef } from 'react';
import type { ReactNode } from 'react';
import { fetchJobMetrics } from '../api/api';

export interface JobCounts {
  running: number;
  succeeded: number;
  failed: number;
  pending: number;
  retrying: number;
}

export interface JobMetricsHistoryItem extends JobCounts {
  timestamp: number;
}

interface JobMetricsContextValue extends JobCounts {
  history: JobMetricsHistoryItem[];
}

const JobMetricsContext = createContext<JobMetricsContextValue>({
  running: 0,
  succeeded: 0,
  failed: 0,
  pending: 0,
  retrying: 0,
  history: [],
});

export const JobMetricsProvider = ({ children }: { children: ReactNode }) => {
  const [jobCounts, setJobCounts] = useState<JobCounts>({
    running: 0,
    succeeded: 0,
    failed: 0,
    pending: 0,
    retrying: 0,
  });
  const [history, setHistory] = useState<JobMetricsHistoryItem[]>([]);
  const lastTimestampRef = useRef<number>(0);

  useEffect(() => {
    let mounted = true;
    const fetchMetrics = async () => {
      const data = await fetchJobMetrics();
      const now = Date.now();
      if (mounted) {
        setJobCounts(data);
        setHistory((prev) => {
          if (
            prev.length === 0 ||
            prev[prev.length - 1].running !== data.running ||
            prev[prev.length - 1].pending !== data.pending ||
            now - lastTimestampRef.current > 1000
          ) {
            lastTimestampRef.current = now;
            const newHistory = [...prev, { ...data, timestamp: now }];
            return newHistory.length > 1000 ? newHistory.slice(newHistory.length - 1000) : newHistory;
          }
          return prev;
        });
      }
    };
    fetchMetrics();
    const interval = setInterval(fetchMetrics, 5000);
    return () => {
      mounted = false;
      clearInterval(interval);
    };
  }, []);

  return (
    <JobMetricsContext.Provider value={{ ...jobCounts, history }}>
      {children}
    </JobMetricsContext.Provider>
  );
};

export const useJobMetrics = () => {
  return useContext(JobMetricsContext);
};
