import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import { useApi } from '../api/api';
import type { Recurrence } from '../api/api.model';
import RecurrencesTable from '../components/recurrences/RecurrencesTable';
import PageHeader from '../components/layout/PageHeader';

const RecurrencesPage = () => {
    const api = useApi();
    const [rows, setRows] = useState<Recurrence[]>([]);
    const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });
    const [rowCount, setRowCount] = useState<number>(0);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        setLoading(true);
        api.fetchRecurrences(paginationModel.page + 1).then(data => {
            setRows(data.items || []);
            setRowCount(data.totalItems || 0);
            setLoading(false);
            setPaginationModel(prev => ({ ...prev, pageSize: data.pageSize }));
        });
    }, [api, paginationModel.page]);

    const handleRefresh = async () => {
        setLoading(true);
        const data = await api.fetchRecurrences(paginationModel.page + 1);
        setRows(data.items || []);
        setRowCount(data.totalItems || 0);
        setLoading(false);
    };

    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
            <PageHeader
                title="Recurrences"
                onRefresh={handleRefresh}
            />
            <Box sx={{ flex: 1, minHeight: 0, p: 3, display: 'flex', flexDirection: 'column' }}>
                <RecurrencesTable
                    rows={rows}
                    rowCount={rowCount}
                    loading={loading}
                    paginationModel={paginationModel}
                    setPaginationModel={setPaginationModel}
                />
            </Box>
        </Box>
    );
};

export default RecurrencesPage;
