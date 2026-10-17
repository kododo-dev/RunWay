import { Suspense, lazy } from 'react';
import Box from '@mui/material/Box';
import PageHeader from '../components/layout/PageHeader';
import DashboardTiles from '../components/dashboard/DashboardTiles';
import { useJobMetrics } from '../context/JobMetricsContext';

// recharts is most of the dashboard's code; the tiles show while it loads.
const ChartJobsHistory = lazy(() => import('../components/dashboard/ChartJobsHistory'));

const DashboardPage = () => {
  const { history, ...jobCounts } = useJobMetrics();

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <PageHeader title="Dashboard" />
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
