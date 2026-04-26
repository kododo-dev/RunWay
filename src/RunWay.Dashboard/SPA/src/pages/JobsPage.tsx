import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import { fetchJobs } from '../api/api';
import { type JobCategory } from '../api/api.model';
import type { JobListItem } from '../api/api.model';
import JobsTable from '../components/jobs/JobsTable';
import PageHeader from '../components/layout/PageHeader';

const pathToJobCategory: Record<string, JobCategory> = {
  pending:   'Pending',
  running:   'Running',
  retrying:  'Retrying',
  succeeded: 'Succeeded',
  failed:    'Failed',
};

const categoryLabel: Record<string, string> = {
  pending:   'Pending jobs',
  running:   'Running jobs',
  retrying:  'Retrying jobs',
  succeeded: 'Succeeded jobs',
  failed:    'Failed jobs',
};

const JobsPage = () => {
  const { category } = useParams<{ category?: string }>();
  const jobCategoryFromPath = category || null;

  const [rows, setRows] = useState<JobListItem[]>([]);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });
  const [rowCount, setRowCount] = useState<number>(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (jobCategoryFromPath && pathToJobCategory[jobCategoryFromPath] !== undefined) {
      setLoading(true);
      fetchJobs(pathToJobCategory[jobCategoryFromPath], paginationModel.page + 1).then(data => {
        setRows(data.items || []);
        setRowCount(data.totalItems || 0);
        setLoading(false);
        setPaginationModel(prev => ({ ...prev, pageSize: data.pageSize }));
      });
    }
  }, [jobCategoryFromPath, paginationModel.page]);

  const handleRefresh = async () => {
    if (!jobCategoryFromPath || pathToJobCategory[jobCategoryFromPath] === undefined) return;
    setLoading(true);
    const data = await fetchJobs(pathToJobCategory[jobCategoryFromPath], paginationModel.page + 1);
    setRows(data.items || []);
    setRowCount(data.totalItems || 0);
    setLoading(false);
  };

  if (!jobCategoryFromPath || pathToJobCategory[jobCategoryFromPath] === undefined) return null;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <PageHeader
        title={categoryLabel[jobCategoryFromPath] ?? 'Jobs'}
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
