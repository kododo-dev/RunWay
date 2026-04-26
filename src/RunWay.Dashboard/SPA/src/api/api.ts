import type {
    JobListItem,
    JobMetrics,
    JobCategory,
    PagedResult,
    RunnerDetails,
    Runner,
    Recurrence
} from "./api.model";

export async function fetchJobMetrics(): Promise<JobMetrics> {
    return await fetch('api/GetStatusesCounts', { method: 'POST' })
        .then((res) => res.json())
        .then((data) => data as JobMetrics);
}

export async function fetchJobs(category: JobCategory, page: number = 1): Promise<PagedResult<JobListItem>> {
    return await fetch('api/GetJobs', {
        method: 'POST',
        body: JSON.stringify({ category, page }),
        headers: { 'Content-Type': 'application/json' }})
        .then((res) => res.json());
}

export async function fetchJobDetails(id: string) {
    return await fetch('api/GetJobDetails', {
        method: 'POST',
        body: JSON.stringify({ id }),
        headers: { 'Content-Type': 'application/json' }})
        .then((res) => res.json());
}

export async function fetchRunners(): Promise<Runner[]> {
    return await fetch('api/GetRunners', { method: 'POST' })
        .then((res) => res.json());
}

export async function fetchRunner(id: string): Promise<RunnerDetails> {
    return await fetch('api/GetRunnerDetails', {
        method: 'POST',
        body: JSON.stringify({ id }),
        headers: { 'Content-Type': 'application/json' },
    }).then((res) => res.json());
}

export async function fetchRecurrences(page: number = 1): Promise<PagedResult<Recurrence>> {
    return await fetch('api/GetRecurrences', {
        method: 'POST',
        body: JSON.stringify({ page }),
        headers: { 'Content-Type': 'application/json' },
    }).then((res) => res.json());
}

export async function fetchRecurrence(id: string): Promise<Recurrence> {
    return await fetch('api/GetRecurrence', {
        method: 'POST',
        body: JSON.stringify({ id }),
        headers: { 'Content-Type': 'application/json' },
    }).then((res) => res.json());
}

export async function fetchRecurrenceJobs(id: string, page: number = 1): Promise<PagedResult<JobListItem>> {
    return await fetch('api/GetRecurrenceJobs', {
        method: 'POST',
        body: JSON.stringify({ id, page }),
        headers: { 'Content-Type': 'application/json' },
    }).then((res) => res.json());
}

export async function requeueJob(id: string): Promise<void> {
    const res = await fetch('api/RequeueJob', {
        method: 'POST',
        body: JSON.stringify({ id }),
        headers: { 'Content-Type': 'application/json' },
    });
    if (!res.ok) throw new Error(`Requeue failed: ${res.status}`);
}
