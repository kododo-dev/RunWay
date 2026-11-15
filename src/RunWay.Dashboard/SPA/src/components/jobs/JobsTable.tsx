import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import dayjs from 'dayjs';
import { type JobListItem, type JobCategory } from '../../api/api.model';
import { useWayNavigate } from '../../utils/useWayNavigate';
import { useTheme } from '@mui/material/styles';
import JobStatusBadge from './JobStatusBadge';
import { useI18n } from '../../i18n/i18n';
import type { Language, TranslationKeys } from '../../i18n/translations';
import { dataGridLocaleText, fromNow } from '../../i18n/dataGrid';

interface JobsTableProps {
  rows: JobListItem[];
  rowCount: number;
  loading: boolean;
  paginationModel: { page: number; pageSize: number };
  setPaginationModel: (model: { page: number; pageSize: number }) => void;
  category: JobCategory;
  hideType?: boolean;
}

function getColumns(category: JobCategory, hideType: boolean, t: TranslationKeys, lang: Language): GridColDef[] {
  const idColumn: GridColDef = {
    field: 'id',
    headerName: t.columnId,
    width: 60,
    sortable: false,
    filterable: false,
    renderCell: (params) => `#${params.row.id}`,
  };

  const typeColumn: GridColDef = {
    field: 'type',
    headerName: t.columnType,
    width: 220,
    sortable: false,
    filterable: false,
    renderCell: (params) => (<span title={params.row.type.fullName}>{params.row.type.name}</span>),
  };

  const priorityColumn: GridColDef   = { field: 'priority',     headerName: t.columnPriority, width: 100, sortable: false, filterable: false };
  const retriesCountColumn: GridColDef = { field: 'retriesCount', headerName: t.columnRetries,  width: 100, sortable: false, filterable: false };

  const modifiedAtColumn: GridColDef = {
    field: 'modifiedAt',
    headerName: t.columnLastUpdate,
    width: 180,
    sortable: false,
    filterable: false,
    renderCell: (params) => fromNow(params.row.modifiedAt, lang),
  };

  const scheduledAtColumn: GridColDef = {
    field: 'scheduledAt',
    headerName: t.columnScheduledAt,
    width: 200,
    sortable: false,
    filterable: false,
    renderCell: (params) => {
      const d = dayjs(params.row.scheduledAt);
      return d.isValid() ? d.toDate().toLocaleString(lang) : '—';
    },
  };

  const statusColumn: GridColDef = {
    field: 'status',
    headerName: t.columnStatus,
    width: 130,
    sortable: false,
    filterable: false,
    renderCell: (params) => <JobStatusBadge status={params.row.status} />,
  };

  const type = hideType ? [] : [typeColumn];
  switch (category) {
    case 'Pending':  return [idColumn, ...type, priorityColumn, scheduledAtColumn];
    case 'Running':  return [idColumn, ...type, scheduledAtColumn, modifiedAtColumn];
    case 'Retrying': return [idColumn, ...type, statusColumn, retriesCountColumn, scheduledAtColumn, modifiedAtColumn];
    default:         return [idColumn, ...type, modifiedAtColumn];
  }
}

const JobsTable = ({ category, rows, rowCount, loading, paginationModel, setPaginationModel, hideType = false }: JobsTableProps) => {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const navigate = useWayNavigate();
  const { t, lang } = useI18n();
  const [columns, setColumns] = useState<GridColDef[]>([]);

  useEffect(() => {
    setColumns(getColumns(category, hideType, t, lang));
  }, [category, hideType, t, lang]);

  const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';
  const headerBg    = isDark ? '#222'    : '#fafafa';
  const rowEvenBg   = isDark ? '#1e1e1e' : '#f9f9fc';
  const cellColor   = isDark ? '#ccc'    : '#333';

  return (
    <DataGrid
      density="compact"
      getRowHeight={() => 36}
      rows={rows}
      columns={columns}
      localeText={dataGridLocaleText(lang)}
      onColumnResize={(params) => {
        setColumns(prev =>
          prev.map(col => col.field === params.colDef.field ? { ...col, width: params.width } : col)
        );
      }}
      disableColumnSelector
      disableDensitySelector
      disableColumnMenu
      hideFooterSelectedRowCount
      isRowSelectable={() => false}
      rowCount={rowCount}
      loading={loading}
      pagination
      paginationMode="server"
      paginationModel={paginationModel}
      onPaginationModelChange={setPaginationModel}
      onRowClick={(params) => navigate(`/jobs/${params.row.id}`)}
      sx={{
        width: '100%',
        height: '100%',
        border: `1px solid ${borderColor}`,
        borderRadius: '6px',
        fontFamily: "'IBM Plex Mono', monospace",
        fontSize: '0.78rem',
        color: cellColor,
        background: isDark ? '#1a1a1a' : '#fff',
        '& .MuiDataGrid-columnHeaders': {
          background: headerBg,
          borderBottom: `1px solid ${borderColor}`,
          fontSize: '0.72rem',
          color: theme.palette.text.secondary,
          textTransform: 'uppercase',
          letterSpacing: '0.05em',
        },
        '& .MuiDataGrid-row': {
          cursor: 'pointer',
        },
        '& .MuiDataGrid-row:nth-of-type(even)': {
          background: rowEvenBg,
        },
        '& .MuiDataGrid-row:hover': {
          background: isDark ? '#2a2a2a' : '#f0f4ff',
        },
        '& .MuiDataGrid-cell': {
          borderColor,
          color: cellColor,
          fontSize: '0.78rem',
        },
        '& .MuiDataGrid-footerContainer': {
          borderTop: `1px solid ${borderColor}`,
          background: headerBg,
        },
        '& .MuiTablePagination-root': {
          fontFamily: "'IBM Plex Mono', monospace",
          fontSize: '0.72rem',
          color: theme.palette.text.secondary,
        },
      }}
    />
  );
};

export default JobsTable;
