import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { fetchRecurrence, fetchRecurrenceJobs } from '../api/api';
import type { Recurrence, JobListItem } from '../api/api.model';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import Divider from '@mui/material/Divider';
import { useTheme } from '@mui/material/styles';
import PageHeader from '../components/layout/PageHeader';
import JobsTable from '../components/jobs/JobsTable';

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };

const RecurrencePage = () => {
    const { id } = useParams<{ id: string }>();
    const theme = useTheme();
    const isDark = theme.palette.mode === 'dark';

    const [recurrence, setRecurrence] = useState<Recurrence | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [jobRows, setJobRows] = useState<JobListItem[]>([]);
    const [jobRowCount, setJobRowCount] = useState<number>(0);
    const [jobPaginationModel, setJobPaginationModel] = useState({ page: 0, pageSize: 10 });
    const [jobsLoading, setJobsLoading] = useState(false);

    const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';
    const cardBg      = isDark ? '#222'    : '#fff';
    const sectionBg   = isDark ? '#1e1e1e' : '#fafafa';

    const fetchAndSetRecurrence = useCallback(() => {
        if (!id) return;
        fetchRecurrence(id)
            .then(data => { setRecurrence(data); setError(null); })
            .catch(() => setError('Error while fetching recurrence details.'))
            .finally(() => setLoading(false));
    }, [id]);

    const fetchJobs = useCallback((page: number) => {
        if (!id) return;
        setJobsLoading(true);
        fetchRecurrenceJobs(id, page).then(data => {
            setJobRows(data.items || []);
            setJobRowCount(data.totalItems || 0);
            setJobPaginationModel(prev => ({ ...prev, pageSize: data.pageSize }));
            setJobsLoading(false);
        });
    }, [id]);

    useEffect(() => {
        setLoading(true);
        fetchAndSetRecurrence();
    }, [fetchAndSetRecurrence]);

    useEffect(() => {
        fetchJobs(jobPaginationModel.page + 1);
    }, [fetchJobs, jobPaginationModel.page]);

    const handleRefresh = () => {
        fetchAndSetRecurrence();
        fetchJobs(jobPaginationModel.page + 1);
    };

    const renderContent = () => {
        if (loading) return (
            <Box sx={{ display: 'flex', justifyContent: 'center', pt: 6 }}>
                <CircularProgress size={24} />
            </Box>
        );
        if (error) return <Alert severity="error" sx={{ m: 3, ...MONO, fontSize: '0.78rem' }}>{error}</Alert>;
        if (!recurrence) return <Alert severity="warning" sx={{ m: 3, ...MONO, fontSize: '0.78rem' }}>Recurrence not found.</Alert>;

        let parsedData: unknown;
        try { parsedData = recurrence.data ? JSON.parse(recurrence.data) : null; }
        catch { parsedData = recurrence.data; }

        return (
            <Box sx={{ p: 3, display: 'flex', flexDirection: 'column', gap: 3 }}>
                <Box sx={{
                    background: cardBg,
                    border: `1px solid ${borderColor}`,
                    borderRadius: '6px',
                    p: 2.5,
                }}>
                    <Box sx={{ mb: 1 }}>
                        <Typography sx={{ ...MONO, fontSize: '1rem', fontWeight: 700, color: theme.palette.text.primary }}>
                            {recurrence.id}
                        </Typography>
                        <Typography sx={{ ...MONO, fontSize: '0.72rem', color: theme.palette.text.secondary, mt: 0.5 }}>
                            {recurrence.type.fullName}
                        </Typography>
                    </Box>
                    <Divider sx={{ borderColor, my: 1.5 }} />
                    <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
                        <Box>
                            <Typography sx={{ ...MONO, fontSize: '0.62rem', color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: '0.06em' }}>Cron</Typography>
                            <Typography sx={{ ...MONO, fontSize: '0.85rem', color: theme.palette.text.primary }}>{recurrence.rule}</Typography>
                        </Box>
                    </Box>
                </Box>

                <Box>
                    <Typography sx={{ ...MONO, fontSize: '0.72rem', color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: '0.08em', mb: 1 }}>
                        Payload
                    </Typography>
                    <Box sx={{
                        background: sectionBg,
                        border: `1px solid ${borderColor}`,
                        borderRadius: '6px',
                        p: 2,
                    }}>
                        <Box
                            component="pre"
                            sx={{
                                m: 0,
                                fontSize: '0.8rem',
                                fontFamily: "'IBM Plex Mono', 'Consolas', monospace",
                                whiteSpace: 'pre-wrap',
                                wordBreak: 'break-word',
                            }}
                        >
                            {JSON.stringify(parsedData ?? {}, null, 2)}
                        </Box>
                    </Box>
                </Box>

                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, minHeight: 400 }}>
                    <Typography sx={{ ...MONO, fontSize: '0.72rem', color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: '0.08em' }}>
                        Jobs
                    </Typography>
                    <Box sx={{ flex: 1, minHeight: 400 }}>
                        <JobsTable
                            rows={jobRows}
                            rowCount={jobRowCount}
                            loading={jobsLoading}
                            paginationModel={jobPaginationModel}
                            setPaginationModel={setJobPaginationModel}
                            category="Retrying"
                            hideType
                        />
                    </Box>
                </Box>
            </Box>
        );
    };

    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
            <PageHeader title={`Recurrence #${id}`} onRefresh={handleRefresh} />
            <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto' }}>
                {renderContent()}
            </Box>
        </Box>
    );
};

export default RecurrencePage;
