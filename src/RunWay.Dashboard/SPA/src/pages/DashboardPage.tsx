import Box from '@mui/material/Box';
import PageHeader from '../components/layout/PageHeader';
import DashboardTiles from '../components/dashboard/DashboardTiles';
import ChartJobsHistory from '../components/dashboard/ChartJobsHistory';
import { useJobMetrics } from '../context/JobMetricsContext';

const DashboardPage = () => {
  const { history, ...jobCounts } = useJobMetrics();

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <PageHeader title="Dashboard" />
      <Box sx={{ flex: 1, overflowY: 'auto' }}>
        <DashboardTiles jobCounts={jobCounts} />
        <ChartJobsHistory history={history} />
      </Box>
    </Box>
  );
};

export default DashboardPage;
