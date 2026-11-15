import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import { useApi } from '../api/api';
import { type JobCategory } from '../api/api.model';
import type { JobListItem } from '../api/api.model';
import JobsTable from '../components/jobs/JobsTable';
import PageHeader from '../components/layout/PageHeader';
import { useI18n } from '../i18n/i18n';
import type { TranslationKeys } from '../i18n/translations';

const pathToJobCategory: Record<string, JobCategory> = {
  pending:   'Pending',
  running:   'Running',
  retrying:  'Retrying',
  succeeded: 'Succeeded',
  failed:    'Failed',
};

const categoryLabel = (t: TranslationKeys): Record<string, string> => ({
  pending:   t.pendingJobs,
  running:   t.runningJobs,
  retrying:  t.retryingJobs,
  succeeded: t.succeededJobs,
  failed:    t.failedJobs,
});

const JobsPage = () => {
  const { category } = useParams<{ category?: string }>();
  const api = useApi();
  const { t } = useI18n();
  const jobCategoryFromPath = category || null;

  const [rows, setRows] = useState<JobListItem[]>([]);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });
  const [rowCount, setRowCount] = useState<number>(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (jobCategoryFromPath && pathToJobCategory[jobCategoryFromPath] !== undefined) {
      setLoading(true);
      api.fetchJobs(pathToJobCategory[jobCategoryFromPath], paginationModel.page + 1).then(data => {
        setRows(data.items || []);
        setRowCount(data.totalItems || 0);
        setLoading(false);
        setPaginationModel(prev => ({ ...prev, pageSize: data.pageSize }));
      });
    }
  }, [api, jobCategoryFromPath, paginationModel.page]);

  const handleRefresh = async () => {
    if (!jobCategoryFromPath || pathToJobCategory[jobCategoryFromPath] === undefined) return;
    setLoading(true);
    const data = await api.fetchJobs(pathToJobCategory[jobCategoryFromPath], paginationModel.page + 1);
    setRows(data.items || []);
    setRowCount(data.totalItems || 0);
    setLoading(false);
  };

  if (!jobCategoryFromPath || pathToJobCategory[jobCategoryFromPath] === undefined) return null;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <PageHeader
        title={categoryLabel(t)[jobCategoryFromPath] ?? t.jobs}
        onRefresh={handleRefresh}
      />
      <Box sx={{ flex: 1, minHeight: 0, p: 3, display: 'flex', flexDirection: 'column' }}>
        <JobsTable
          rows={rows}
          rowCount={rowCount}
          loading={loading}
          paginationModel={paginationModel}
          setPaginationModel={setPaginationModel}
          category={pathToJobCategory[jobCategoryFromPath]}
        />
      </Box>
    </Box>
  );
};

export default JobsPage;
