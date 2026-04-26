import { useLocation, useNavigate } from 'react-router-dom';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Divider from '@mui/material/Divider';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import Badge from '@mui/material/Badge';
import DashboardIcon from '@mui/icons-material/Dashboard';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import AutorenewIcon from '@mui/icons-material/Autorenew';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import RotateRightIcon from '@mui/icons-material/RotateRight';
import DirectionsRunIcon from '@mui/icons-material/DirectionsRun';
import RepeatIcon from '@mui/icons-material/Repeat';
import { useTheme } from '@mui/material/styles';
import { useJobMetrics } from '../../context/JobMetricsContext';
import { useRunners } from '../../context/RunnersContext';
import { useRecurrences } from '../../context/RecurrencesContext';
import { CATEGORY_COLORS } from '../../utils/jobUtils';

const jobCategories = [
  { key: 'pending',   label: 'Pending',   icon: <HourglassEmptyIcon />, color: CATEGORY_COLORS.pending },
  { key: 'running',   label: 'Running',   icon: <AutorenewIcon />,      color: CATEGORY_COLORS.running },
  { key: 'retrying',  label: 'Retrying',  icon: <RotateRightIcon />,    color: CATEGORY_COLORS.retrying },
  { key: 'succeeded', label: 'Succeeded', icon: <CheckCircleOutlineIcon />, color: CATEGORY_COLORS.succeeded },
  { key: 'failed',    label: 'Failed',    icon: <ErrorOutlineIcon />,      color: CATEGORY_COLORS.failed },
];

const SideMenu = ({ onNavigate }: { onNavigate?: () => void }) => {
  const location = useLocation();
  const navigate = useNavigate();
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const { history: _history, ...jobCounts } = useJobMetrics();
  const { onlineCount: runnersCount } = useRunners();
  const { totalCount: recurrencesCount } = useRecurrences();

  const path = location.pathname;
  const isDashboard = path === '/' || path === '/dashboard';
  const isRunners = path === '/runners';
  const isRecurrences = path === '/recurrences' || path.startsWith('/recurrences/');
  const jobCategoryFromPath = path.startsWith('/jobs/category/') ? path.split('/')[3] : null;

  const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';

  const iconSx       = { color: isDark ? '#666' : '#aaa', minWidth: 32, '& svg': { fontSize: 16 } };
  const activeIconSx = { ...iconSx, color: theme.palette.primary.main };

  const textSx = (active: boolean) => ({
    '& .MuiListItemText-primary': {
      fontSize: '0.8rem',
      fontFamily: "'IBM Plex Mono', monospace",
      color: active ? theme.palette.text.primary : theme.palette.text.secondary,
      fontWeight: active ? 600 : 400,
    },
  });

  const neutralBadgeSx = {
    mr: 1.5,
    '& .MuiBadge-badge': {
      backgroundColor: isDark ? '#444' : '#ddd',
      color: isDark ? '#ccc' : '#555',
      fontSize: '0.6rem',
      minWidth: 16,
      height: 16,
      fontFamily: "'IBM Plex Mono', monospace",
    },
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'hidden' }}>
      <Box sx={{ flexShrink: 0, py: 1 }}>
        <List dense disablePadding>
          <ListItem disablePadding>
            <ListItemButton selected={isDashboard} onClick={() => { navigate('/'); onNavigate?.(); }}>
              <ListItemIcon sx={isDashboard ? activeIconSx : iconSx}><DashboardIcon /></ListItemIcon>
              <ListItemText primary="Dashboard" sx={textSx(isDashboard)} />
            </ListItemButton>
          </ListItem>
        </List>
      </Box>

      <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'hidden' }}>
        <Divider sx={{ borderColor, mx: 2, flexShrink: 0 }} />

        <Typography sx={{
          px: 2.5, pt: 1, pb: 0.5,
          fontSize: '0.62rem',
          color: isDark ? '#444' : '#bbb',
          textTransform: 'uppercase',
          letterSpacing: '0.1em',
          fontFamily: 'monospace',
          flexShrink: 0,
        }}>
          Jobs
        </Typography>

        <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto', pb: 1 }}>
          <List dense disablePadding>
            {jobCategories.map(cat => {
              const active = jobCategoryFromPath === cat.key;
              const count = jobCounts[cat.key as keyof typeof jobCounts] ?? 0;
              return (
                <ListItem disablePadding key={cat.key}>
                  <ListItemButton
                    selected={active}
                    onClick={() => { navigate(`/jobs/category/${cat.key}`); onNavigate?.(); }}
                  >
                    <ListItemIcon sx={active ? { ...activeIconSx, color: cat.color } : { ...iconSx, '& svg': { fontSize: 16 } }}>
                      {cat.icon}
                    </ListItemIcon>
                    <ListItemText primary={cat.label} sx={textSx(active)} />
                    {count > 0 && (
                      <Badge
                        badgeContent={count}
                        sx={{
                          mr: 1.5,
                          '& .MuiBadge-badge': {
                            backgroundColor: cat.color,
                            color: '#fff',
                            fontSize: '0.6rem',
                            minWidth: 16,
                            height: 16,
                            fontFamily: "'IBM Plex Mono', monospace",
                          },
                        }}
                      />
                    )}
                  </ListItemButton>
                </ListItem>
              );
            })}


            <Divider sx={{ borderColor, mx: 2, my: 0.5 }} />

            <ListItem disablePadding>
              <ListItemButton selected={isRecurrences} onClick={() => { navigate('/recurrences'); onNavigate?.(); }}>
                <ListItemIcon sx={isRecurrences ? activeIconSx : iconSx}><RepeatIcon /></ListItemIcon>
                <ListItemText primary="Recurrences" sx={textSx(isRecurrences)} />
                {recurrencesCount > 0 && (
                  <Badge badgeContent={recurrencesCount} sx={neutralBadgeSx} />
                )}
              </ListItemButton>
            </ListItem>

            <Divider sx={{ borderColor, mx: 2, my: 0.5 }} />

            <ListItem disablePadding>
              <ListItemButton selected={isRunners} onClick={() => { navigate('/runners'); onNavigate?.(); }}>
                <ListItemIcon sx={isRunners ? activeIconSx : iconSx}><DirectionsRunIcon /></ListItemIcon>
                <ListItemText primary="Runners" sx={textSx(isRunners)} />
                {runnersCount > 0 && (
                  <Badge badgeContent={runnersCount} sx={neutralBadgeSx} />
                )}
              </ListItemButton>
            </ListItem>
          </List>
        </Box>
      </Box>
    </Box>
  );
};

export default SideMenu;
