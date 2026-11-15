import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import { type Recurrence } from '../../api/api.model';
import { useTheme } from '@mui/material/styles';
import { useWayNavigate } from '../../utils/useWayNavigate';
import { useI18n } from '../../i18n/i18n';
import type { TranslationKeys } from '../../i18n/translations';
import { dataGridLocaleText } from '../../i18n/dataGrid';

interface RecurrencesTableProps {
    rows: Recurrence[];
    rowCount: number;
    loading: boolean;
    paginationModel: { page: number; pageSize: number };
    setPaginationModel: (model: { page: number; pageSize: number }) => void;
}

const getColumns = (t: TranslationKeys): GridColDef[] => [
    {
        field: 'id',
        headerName: t.columnId,
        width: 160,
        sortable: false,
        filterable: false,
        renderCell: (params) => params.row.id,
    },
    {
        field: 'type',
        headerName: t.columnType,
        width: 220,
        sortable: false,
        filterable: false,
        renderCell: (params) => (
            <span title={params.row.type.fullName}>{params.row.type.name}</span>
        ),
    },
    {
        field: 'rule',
        headerName: t.columnCron,
        width: 200,
        sortable: false,
        filterable: false,
        renderCell: (params) => params.row.rule ?? '—',
    },
];

const RecurrencesTable = ({ rows, rowCount, loading, paginationModel, setPaginationModel }: RecurrencesTableProps) => {
    const theme = useTheme();
    const isDark = theme.palette.mode === 'dark';
    const navigate = useWayNavigate();
    const { t, lang } = useI18n();

    const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';
    const headerBg    = isDark ? '#222'    : '#fafafa';
    const rowEvenBg   = isDark ? '#1e1e1e' : '#f9f9fc';
    const cellColor   = isDark ? '#ccc'    : '#333';

    return (
        <DataGrid
            density="compact"
            getRowHeight={() => 36}
            rows={rows}
            columns={getColumns(t)}
            localeText={dataGridLocaleText(lang)}
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
            onRowClick={(params) => navigate(`/recurrences/${params.row.id}`)}
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

export default RecurrencesTable;
