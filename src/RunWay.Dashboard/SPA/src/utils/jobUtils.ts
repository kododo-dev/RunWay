import { type JobStatus, type JobAuditItemType } from '../api/api.model';

export const PendingColor = '#8e24aa';
export const RunningColor = '#2196f3';
export const SucceededColor = '#43a047';
export const FailedColor = '#e53935';
export const RetryingColor = '#fb8c00';

export const STATUS_COLORS: Record<JobStatus, string> = {
    ['Pending']: PendingColor,
    ['Running']: RunningColor,
    ['Succeeded']: SucceededColor,
    ['Failed']: FailedColor,
};

export const CATEGORY_COLORS: Record<string, string> = {
    pending: PendingColor,
    running: RunningColor,
    retrying: RetryingColor,
    succeeded: SucceededColor,
    failed: FailedColor,
};

export const statusName = (status: JobStatus): string => {
    switch (status) {
        case 'Pending': return 'Scheduled';
        case 'Running': return 'Running';
        case 'Succeeded': return 'Succeeded';
        case 'Failed': return 'Failed';
        default: return String(status);
    }
};

export const auditTypeName = (type: JobAuditItemType): string => {
    switch (type) {
        case 'Created': return 'Created';
        case 'Scheduled': return 'Scheduled';
        case 'Started': return 'Started';
        case 'Succeeded': return 'Succeeded';
        case 'Failed': return 'Failed';
        default: return String(type);
    }
};

export const auditDotColor = (type: JobAuditItemType): 'success' | 'error' | 'warning' | 'primary' => {
    switch (type) {
        case 'Succeeded': return 'success';
        case 'Failed': return 'error';
        default: return 'primary';
    }
};

export const formatTimeDelta = (ms: number): string => {
    const sec = Math.abs(Math.round(ms / 1000));
    if (sec < 60) return `+${sec}s`;
    const min = Math.floor(sec / 60);
    const rem = sec % 60;
    return `+${min}m${rem > 0 ? ' ' + rem + 's' : ''}`;
};

export const localDate = (date: string): string => {
    if (!date) return '';
    const d = new Date(date);
    return isNaN(d.getTime()) ? date : d.toLocaleString();
};
