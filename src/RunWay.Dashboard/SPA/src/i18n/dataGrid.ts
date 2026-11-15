import { plPL } from '@mui/x-data-grid/locales';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import 'dayjs/locale/pl';
import type { Language } from './translations';

dayjs.extend(relativeTime);

// Only the tables import this file, so the data grid's and dayjs' Polish texts load with the tables, not the dashboard.

/** The data grid's own texts (pagination, "no rows") in the dashboard's language; undefined keeps English. */
export const dataGridLocaleText = (lang: Language) =>
  lang === 'pl' ? plPL.components.MuiDataGrid.defaultProps.localeText : undefined;

/** "5 minutes ago" in the dashboard's language. */
export const fromNow = (date: string, lang: Language): string => {
  const d = dayjs(date);
  return d.isValid() ? d.locale(lang).fromNow() : '—';
};
