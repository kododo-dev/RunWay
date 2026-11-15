export type Language = 'en' | 'pl';

const en = {
  // Menu and page titles
  dashboard: 'Dashboard',
  jobs: 'Jobs',
  recurrences: 'Recurrences',
  runners: 'Runners',
  refresh: 'Refresh',
  loading: 'Loading...',

  // Job categories (menu, tiles, chart)
  categoryPending: 'Pending',
  categoryRunning: 'Running',
  categoryRetrying: 'Retrying',
  categorySucceeded: 'Succeeded',
  categoryFailed: 'Failed',

  // Titles of the job lists
  pendingJobs: 'Pending jobs',
  runningJobs: 'Running jobs',
  retryingJobs: 'Retrying jobs',
  succeededJobs: 'Succeeded jobs',
  failedJobs: 'Failed jobs',

  // Job statuses
  statusPending: 'Scheduled',
  statusRunning: 'Running',
  statusSucceeded: 'Succeeded',
  statusFailed: 'Failed',

  // Audit entries
  auditCreated: 'Created',
  auditScheduled: 'Scheduled',
  auditStarted: 'Started',
  auditSucceeded: 'Succeeded',
  auditFailed: 'Failed',

  // Table columns
  columnId: 'ID',
  columnType: 'Type',
  columnPriority: 'Priority',
  columnRetries: 'Retries',
  columnLastUpdate: 'Last Update',
  columnScheduledAt: 'Scheduled At',
  columnStatus: 'Status',
  columnCron: 'Cron',

  // Dashboard
  activeJobsHistory: 'Active jobs history',

  // Job
  job: (id: string) => `Job #${id}`,
  requeue: 'Requeue',
  requeueFailed: 'Requeue failed. Please try again.',
  jobFetchError: 'Error while fetching job details.',
  jobNotFound: 'Job not found.',
  priority: 'Priority',
  retries: 'Retries',
  payload: 'Payload',
  auditTimeline: 'Audit timeline',
  noAuditRecords: 'No audit records.',

  // Recurrence
  recurrence: (id: string) => `Recurrence #${id}`,
  recurrenceFetchError: 'Error while fetching recurrence details.',
  recurrenceNotFound: 'Recurrence not found.',
  cron: 'Cron',

  // Runners
  online: 'online',
  offline: 'offline',
  runnerId: 'id',
  lastSeen: 'last seen',
  registered: 'registered',
  noRunners: 'No runners registered.',
  runnerNotFound: 'Runner not found',
  runnerNoLongerRegistered: (id: string) => `Runner #${id} is no longer registered.`,
  details: 'Details',
  jobTypes: 'Job types',
  none: 'None',
  activeJobs: 'Active jobs',
  activeJobsError: 'Could not load active jobs.',
  noActiveJobs: 'No active jobs.',
  secondsAgo: (n: number) => `${n}s ago`,
  minutesAgo: (n: number) => `${n}m ago`,
  hoursAgo: (n: number) => `${n}h ago`,
  daysAgo: (n: number) => `${n}d ago`,
};

export type TranslationKeys = typeof en;

const pl: TranslationKeys = {
  dashboard: 'Pulpit',
  jobs: 'Zadania',
  recurrences: 'Cykliczne',
  runners: 'Runnery',
  refresh: 'Odśwież',
  loading: 'Ładowanie...',

  categoryPending: 'Oczekujące',
  categoryRunning: 'W trakcie',
  categoryRetrying: 'Ponawiane',
  categorySucceeded: 'Zakończone',
  categoryFailed: 'Nieudane',

  pendingJobs: 'Zadania oczekujące',
  runningJobs: 'Zadania w trakcie',
  retryingJobs: 'Zadania ponawiane',
  succeededJobs: 'Zadania zakończone',
  failedJobs: 'Zadania nieudane',

  statusPending: 'Zaplanowane',
  statusRunning: 'W trakcie',
  statusSucceeded: 'Zakończone',
  statusFailed: 'Nieudane',

  auditCreated: 'Utworzone',
  auditScheduled: 'Zaplanowane',
  auditStarted: 'Rozpoczęte',
  auditSucceeded: 'Zakończone',
  auditFailed: 'Nieudane',

  columnId: 'ID',
  columnType: 'Typ',
  columnPriority: 'Priorytet',
  columnRetries: 'Ponowienia',
  columnLastUpdate: 'Ostatnia zmiana',
  columnScheduledAt: 'Zaplanowane na',
  columnStatus: 'Status',
  columnCron: 'Cron',

  activeJobsHistory: 'Historia aktywnych zadań',

  job: (id: string) => `Zadanie #${id}`,
  requeue: 'Ponów',
  requeueFailed: 'Nie udało się ponowić zadania. Spróbuj ponownie.',
  jobFetchError: 'Błąd podczas pobierania szczegółów zadania.',
  jobNotFound: 'Nie znaleziono zadania.',
  priority: 'Priorytet',
  retries: 'Ponowienia',
  payload: 'Dane zadania',
  auditTimeline: 'Historia zadania',
  noAuditRecords: 'Brak wpisów w historii.',

  recurrence: (id: string) => `Zadanie cykliczne #${id}`,
  recurrenceFetchError: 'Błąd podczas pobierania szczegółów zadania cyklicznego.',
  recurrenceNotFound: 'Nie znaleziono zadania cyklicznego.',
  cron: 'Cron',

  online: 'online',
  offline: 'offline',
  runnerId: 'id',
  lastSeen: 'ostatnio',
  registered: 'od',
  noRunners: 'Brak zarejestrowanych runnerów.',
  runnerNotFound: 'Nie znaleziono runnera',
  runnerNoLongerRegistered: (id: string) => `Runner #${id} nie jest już zarejestrowany.`,
  details: 'Szczegóły',
  jobTypes: 'Typy zadań',
  none: 'Brak',
  activeJobs: 'Aktywne zadania',
  activeJobsError: 'Nie udało się pobrać aktywnych zadań.',
  noActiveJobs: 'Brak aktywnych zadań.',
  secondsAgo: (n: number) => `${n} s temu`,
  minutesAgo: (n: number) => `${n} min temu`,
  hoursAgo: (n: number) => `${n} godz. temu`,
  daysAgo: (n: number) => `${n} dni temu`,
};

export const translations: Record<Language, TranslationKeys> = { en, pl };
