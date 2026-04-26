import { HashRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { ThemeContextProvider, useThemeMode } from './context/ThemeContext';
import { JobMetricsProvider } from './context/JobMetricsContext';
import { RunnersProvider } from './context/RunnersContext';
import { RecurrencesProvider } from './context/RecurrencesContext';
import MainLayout from './components/layout/MainLayout';
import DashboardPage from './pages/DashboardPage';
import JobsPage from './pages/JobsPage';
import JobPage from './pages/JobPage';
import RunnersPage from './pages/RunnersPage';
import RunnerPage from './pages/RunnerPage';
import RecurrencesPage from './pages/RecurrencesPage';
import RecurrencePage from './pages/RecurrencePage';

function AppInner() {
  const { muiTheme } = useThemeMode();
  return (
    <ThemeProvider theme={muiTheme}>
      <CssBaseline />
      <JobMetricsProvider>
        <RunnersProvider>
        <RecurrencesProvider>
        <HashRouter>
          <MainLayout>
            <Routes>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/dashboard" element={<Navigate to="/" replace />} />
              <Route path="/jobs/category/:category" element={<JobsPage />} />
              <Route path="/jobs/:id" element={<JobPage />} />
              <Route path="/recurrences" element={<RecurrencesPage />} />
              <Route path="/recurrences/:id" element={<RecurrencePage />} />
              <Route path="/runners" element={<RunnersPage />} />
              <Route path="/runners/:id" element={<RunnerPage />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </MainLayout>
        </HashRouter>
        </RecurrencesProvider>
        </RunnersProvider>
      </JobMetricsProvider>
    </ThemeProvider>
  );
}

function App() {
  return (
    <ThemeContextProvider>
      <AppInner />
    </ThemeContextProvider>
  );
}

export default App;
