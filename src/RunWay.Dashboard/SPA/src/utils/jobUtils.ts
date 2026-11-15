import { type JobStatus, type JobAuditItemType } from '../api/api.model';
import type { TranslationKeys } from '../i18n/translations';

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

export const statusName = (status: JobStatus, t: TranslationKeys): string => {
    switch (status) {
        case 'Pending': return t.statusPending;
        case 'Running': return t.statusRunning;
        case 'Succeeded': return t.statusSucceeded;
        case 'Failed': return t.statusFailed;
        default: return String(status);
    }
};

export const auditTypeName = (type: JobAuditItemType, t: TranslationKeys): string => {
    switch (type) {
        case 'Created': return t.auditCreated;
        case 'Scheduled': return t.auditScheduled;
        case 'Started': return t.auditStarted;
        case 'Succeeded': return t.auditSucceeded;
        case 'Failed': return t.auditFailed;
        default: return String(type);
    }
};

/** Job categories in the order of the menu and the tiles. */
export const jobCategories = (t: TranslationKeys) => [
    { key: 'pending',   label: t.categoryPending,   color: CATEGORY_COLORS.pending },
    { key: 'running',   label: t.categoryRunning,   color: CATEGORY_COLORS.running },
    { key: 'retrying',  label: t.categoryRetrying,  color: CATEGORY_COLORS.retrying },
    { key: 'succeeded', label: t.categorySucceeded, color: CATEGORY_COLORS.succeeded },
    { key: 'failed',    label: t.categoryFailed,    color: CATEGORY_COLORS.failed },
];

/** "5m ago" for a runner's last heartbeat. */
export const formatRelativeTime = (dateStr: string, t: TranslationKeys): string => {
    const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
    if (diff < 60) return t.secondsAgo(diff);
    if (diff < 3600) return t.minutesAgo(Math.floor(diff / 60));
    if (diff < 86400) return t.hoursAgo(Math.floor(diff / 3600));
    return t.daysAgo(Math.floor(diff / 86400));
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

/** A date and time in the dashboard's language, e.g. 15.11.2026, 19:29:41 in Polish. */
export const localDate = (date: string, lang?: string): string => {
    if (!date) return '';
    const d = new Date(date);
    return isNaN(d.getTime()) ? date : d.toLocaleString(lang);
};
