import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import { type JobListItem, type JobCategory } from '../../api/api.model';
import { useNavigate } from 'react-router-dom';
import { useTheme } from '@mui/material/styles';
import JobStatusBadge from './JobStatusBadge';

dayjs.extend(relativeTime);

interface JobsTableProps {
  rows: JobListItem[];
  rowCount: number;
  loading: boolean;
  paginationModel: { page: number; pageSize: number };
  setPaginationModel: (model: { page: number; pageSize: number }) => void;
  category: JobCategory;
  hideType?: boolean;
}

const idColumn: GridColDef = {
  field: 'id',
  headerName: 'ID',
  width: 60,
  sortable: false,
  filterable: false,
  renderCell: (params) => `#${params.row.id}`,
};

const typeColumn: GridColDef = {
  field: 'type',
  headerName: 'Type',
  width: 220,
  sortable: false,
  filterable: false,
  renderCell: (params) => (<span title={params.row.type.fullName}>{params.row.type.name}</span>),
};

const priorityColumn: GridColDef   = { field: 'priority',     headerName: 'Priority', width: 100, sortable: false, filterable: false };
const retriesCountColumn: GridColDef = { field: 'retriesCount', headerName: 'Retries',  width: 100, sortable: false, filterable: false };

const modifiedAtColumn: GridColDef = {
  field: 'modifiedAt',
  headerName: 'Last Update',
  width: 180,
  sortable: false,
  filterable: false,
  renderCell: (params) => {
    const d = dayjs(params.row.modifiedAt);
    return d.isValid() ? d.fromNow() : '—';
  },
};

const scheduledAtColumn: GridColDef = {
  field: 'scheduledAt',
  headerName: 'Scheduled At',
  width: 200,
  sortable: false,
  filterable: false,
  renderCell: (params) => {
    const d = dayjs(params.row.scheduledAt);
    return d.isValid() ? d.toDate().toLocaleString() : '—';
  },
};

const statusColumn: GridColDef = {
  field: 'status',
  headerName: 'Status',
  width: 130,
  sortable: false,
  filterable: false,
  renderCell: (params) => <JobStatusBadge status={params.row.status} />,
};

function getColumns(category: JobCategory, hideType: boolean): GridColDef[] {
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
  const navigate = useNavigate();
  const [columns, setColumns] = useState<GridColDef[]>([]);

  useEffect(() => {
    setColumns(getColumns(category, hideType));
  }, [category, hideType]);

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
