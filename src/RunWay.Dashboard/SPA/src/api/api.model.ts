export type JobStatus = 'Pending' | 'Running' | 'Succeeded' | 'Failed';
export type JobCategory = 'Pending' | 'Running' | 'Succeeded' | 'Failed' | 'Retrying';
export type JobAuditItemType = 'Created' | 'Scheduled' | 'Started' | 'Succeeded' | 'Failed';
export type RunnerStatus = 'Online' | 'Offline';

export interface JobMetrics {
    running: number;
    succeeded: number;
    failed: number;
    pending: number;
    retrying: number;
}

export interface JobType {
    name: string;
    fullName: string;
}

export interface JobListItem {
    id: string;
    type: JobType;
    modifiedAt: string;
    scheduledAt: string;
    priority: number;
    retriesCount: number;
    status: JobStatus;
}

export interface JobAuditItem {
    time: string,
    type: JobAuditItemType;
    details: string;
}

export interface JobDetails {
    id: string;
    type: JobType;
    data: string;
    modifiedAt: string;
    scheduledAt: string;
    priority: number;
    retriesCount: number;
    status: JobStatus;
    audit: JobAuditItem[];
}

export interface PagedResult<T> {
    items: T[];
    totalItems: number;
    totalPages: number;
    pageSize: number;
}

export interface Recurrence {
    id: string;
    rule: string;
    type: JobType;
    data: string;
}

export interface Runner {
    id: string;
    name: string;
    status: RunnerStatus;
    lastSeenAt: string;
}

export interface RunnerDetails {
    id: string;
    name: string;
    status: RunnerStatus;
    lastSeenAt: string;
    createdAt: string;
    jobTypes: JobType[];
    jobs: JobListItem[];
}