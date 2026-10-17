import { useMemo } from 'react';
import { useShell, type WayApi } from '@kododo-dev/way-shell';
import type {
    JobListItem,
    JobMetrics,
    JobCategory,
    JobDetails,
    PagedResult,
    RunnerDetails,
    Runner,
    Recurrence
} from "./api.model";

/** The dashboard's API, on the shell's client: its apiBase, problem details and expired-session handling. */
export function createRunWayApi(api: WayApi) {
    return {
        fetchJobMetrics: (): Promise<JobMetrics> => api.post('GetStatusesCounts'),
        fetchJobs: (category: JobCategory, page: number = 1): Promise<PagedResult<JobListItem>> =>
            api.post('GetJobs', { category, page }),
        fetchJobDetails: (id: string): Promise<JobDetails> => api.post('GetJobDetails', { id }),
        fetchRunners: (): Promise<Runner[]> => api.post('GetRunners'),
        fetchRunner: (id: string): Promise<RunnerDetails> => api.post('GetRunnerDetails', { id }),
        fetchRecurrences: (page: number = 1): Promise<PagedResult<Recurrence>> => api.post('GetRecurrences', { page }),
        fetchRecurrence: (id: string): Promise<Recurrence> => api.post('GetRecurrence', { id }),
        fetchRecurrenceJobs: (id: string, page: number = 1): Promise<PagedResult<JobListItem>> =>
            api.post('GetRecurrenceJobs', { id, page }),
        requeueJob: (id: string): Promise<void> => api.post('RequeueJob', { id }),
    };
}

export type RunWayApi = ReturnType<typeof createRunWayApi>;

export function useApi(): RunWayApi {
    const { api } = useShell();
    return useMemo(() => createRunWayApi(api), [api]);
}
