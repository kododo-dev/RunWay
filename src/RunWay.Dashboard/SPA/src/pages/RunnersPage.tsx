import { useWayNavigate } from '../utils/useWayNavigate';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { useTheme } from '@mui/material/styles';
import { useRunners } from '../context/RunnersContext';
import type {Runner} from '../api/api.model';
import PageHeader from '../components/layout/PageHeader';
import { useI18n } from '../i18n/i18n';
import { formatRelativeTime } from '../utils/jobUtils';

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };
const ONLINE_THRESHOLD_MS = 120_000;

function isOnline(lastSeenAt: string): boolean {
  return Date.now() - new Date(lastSeenAt).getTime() < ONLINE_THRESHOLD_MS;
}

interface RunnerCardProps {
  runner: Runner;
}

const RunnerCard = ({ runner }: RunnerCardProps) => {
  const navigate = useWayNavigate();
  const theme = useTheme();
  const { t } = useI18n();
  const isDark = theme.palette.mode === 'dark';
  const online = isOnline(runner.lastSeenAt);

  const cardBg      = isDark ? '#242424' : '#fff';
  const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';
  const dimColor    = isDark ? '#555'    : '#aaa';
  const onlineColor  = '#4caf50';
  const offlineColor = '#f44336';

  return (
    <Box
      onClick={() => navigate(`/runners/${runner.id}`)}
      sx={{
        background: cardBg,
        border: `1px solid ${borderColor}`,
        borderRadius: 1,
        p: 2.5,
        display: 'flex',
        flexDirection: 'column',
        gap: 1.25,
        cursor: 'pointer',
        transition: 'border-color 0.15s',
        '&:hover': { borderColor: theme.palette.primary.main },
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Box sx={{
          width: 8, height: 8,
          borderRadius: '50%',
          flexShrink: 0,
          backgroundColor: online ? onlineColor : offlineColor,
          boxShadow: online ? `0 0 6px ${onlineColor}` : 'none',
        }} />
        <Typography sx={{
          ...MONO,
          fontSize: '0.88rem',
          fontWeight: 700,
          color: theme.palette.text.primary,
          flex: 1,
          minWidth: 0,
          overflow: 'hidden',
          textOverflow: 'ellipsis',
          whiteSpace: 'nowrap',
        }}>
          {runner.name}
        </Typography>
        <Typography sx={{ ...MONO, fontSize: '0.68rem', color: online ? onlineColor : offlineColor, fontWeight: 600, flexShrink: 0 }}>
          {online ? t.online : t.offline}
        </Typography>
      </Box>

      <Box sx={{ display: 'flex', gap: 1 }}>
        <Typography sx={{ ...MONO, fontSize: '0.7rem', color: dimColor, minWidth: 72 }}>{t.runnerId}</Typography>
        <Typography sx={{ ...MONO, fontSize: '0.7rem', color: theme.palette.text.secondary }}>{runner.id}</Typography>
      </Box>
      <Box sx={{ display: 'flex', gap: 1 }}>
        <Typography sx={{ ...MONO, fontSize: '0.7rem', color: dimColor, minWidth: 72 }}>{t.lastSeen}</Typography>
        <Typography sx={{ ...MONO, fontSize: '0.7rem', color: theme.palette.text.secondary }}>
          {formatRelativeTime(runner.lastSeenAt, t)}
        </Typography>
      </Box>
    </Box>
  );
};

const RunnersPage = () => {
  const { runners, loaded } = useRunners();
  const theme = useTheme();
  const { t } = useI18n();
  const isDark = theme.palette.mode === 'dark';
  const dimColor = isDark ? '#555' : '#aaa';

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <PageHeader title={t.runners} />
      <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto', p: 3 }}>
        {loaded && runners.length === 0 && (
          <Typography sx={{ ...MONO, fontSize: '0.8rem', color: dimColor }}>
            {t.noRunners}
          </Typography>
        )}
        <Box sx={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
          gap: 2,
        }}>
          {runners.map(runner => (
            <RunnerCard key={runner.id} runner={runner} />
          ))}
        </Box>
      </Box>
    </Box>
  );
};

export default RunnersPage;
