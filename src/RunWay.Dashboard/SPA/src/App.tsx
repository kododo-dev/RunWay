import { Suspense, lazy, useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import { JobMetricsProvider } from './context/JobMetricsContext';
import { RunnersProvider } from './context/RunnersContext';
import { RecurrencesProvider } from './context/RecurrencesContext';
import SideMenu from './components/layout/SideMenu';
import DashboardPage from './pages/DashboardPage';
import RunnersPage from './pages/RunnersPage';
import RunnerPage from './pages/RunnerPage';
import { useWayNavigate } from './utils/useWayNavigate';

// The pages with the data grid (MUI X) and the timeline (Lab) load when opened, so the dashboard does not wait for them.
const JobsPage = lazy(() => import('./pages/JobsPage'));
const JobPage = lazy(() => import('./pages/JobPage'));
const RecurrencesPage = lazy(() => import('./pages/RecurrencesPage'));
const RecurrencePage = lazy(() => import('./pages/RecurrencePage'));

function Loading() {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', p: 6 }}>
      <CircularProgress size={28} />
    </Box>
  );
}

/** Replaces the current address with the dashboard's root, e.g. for an unknown page. */
function ToDashboard() {
  const navigate = useWayNavigate();
  useEffect(() => navigate('/', { replace: true }), [navigate]);
  return null;
}

// The dashboard inside Way.Shell: the shell provides the router, theme and layout; the dashboard's menu goes into the
// shell's side menu.
function App() {
  return (
    <JobMetricsProvider>
      <RunnersProvider>
        <RecurrencesProvider>
          <SideMenu />
          <Suspense fallback={<Loading />}>
            <Routes>
              <Route index element={<DashboardPage />} />
              <Route path="jobs/category/:category" element={<JobsPage />} />
              <Route path="jobs/:id" element={<JobPage />} />
              <Route path="recurrences" element={<RecurrencesPage />} />
              <Route path="recurrences/:id" element={<RecurrencePage />} />
              <Route path="runners" element={<RunnersPage />} />
              <Route path="runners/:id" element={<RunnerPage />} />
              <Route path="*" element={<ToDashboard />} />
            </Routes>
          </Suspense>
        </RecurrencesProvider>
      </RunnersProvider>
    </JobMetricsProvider>
  );
}

export default App;
