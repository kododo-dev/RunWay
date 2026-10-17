import { useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import { JobMetricsProvider } from './context/JobMetricsContext';
import { RunnersProvider } from './context/RunnersContext';
import { RecurrencesProvider } from './context/RecurrencesContext';
import SideMenu from './components/layout/SideMenu';
import DashboardPage from './pages/DashboardPage';
import JobsPage from './pages/JobsPage';
import JobPage from './pages/JobPage';
import RunnersPage from './pages/RunnersPage';
import RunnerPage from './pages/RunnerPage';
import RecurrencesPage from './pages/RecurrencesPage';
import RecurrencePage from './pages/RecurrencePage';
import { useWayNavigate } from './utils/useWayNavigate';

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
        </RecurrencesProvider>
      </RunnersProvider>
    </JobMetricsProvider>
  );
}

export default App;
