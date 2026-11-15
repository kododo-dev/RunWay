import { Suspense, lazy } from 'react';
import Box from '@mui/material/Box';
import PageHeader from '../components/layout/PageHeader';
import DashboardTiles from '../components/dashboard/DashboardTiles';
import { useJobMetrics } from '../context/JobMetricsContext';
import { useI18n } from '../i18n/i18n';

// recharts is most of the dashboard's code; the tiles show while it loads.
const ChartJobsHistory = lazy(() => import('../components/dashboard/ChartJobsHistory'));

const DashboardPage = () => {
  const { history, ...jobCounts } = useJobMetrics();
  const { t } = useI18n();

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <PageHeader title={t.dashboard} />
      <Box sx={{ flex: 1, overflowY: 'auto' }}>
        <DashboardTiles jobCounts={jobCounts} />
        <Suspense>
          <ChartJobsHistory history={history} />
        </Suspense>
      </Box>
    </Box>
  );
};

export default DashboardPage;
