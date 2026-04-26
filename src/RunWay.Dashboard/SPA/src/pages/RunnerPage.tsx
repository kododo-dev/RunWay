import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import RefreshIcon from '@mui/icons-material/Refresh';
import { useTheme } from '@mui/material/styles';
import type {RunnerDetails} from "../api/api.model.ts";
import {fetchRunner} from "../api/api.ts";

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };

function formatRelativeTime(dateStr: string): string {
  const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
  if (diff < 60) return `${diff}s ago`;
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  return `${Math.floor(diff / 86400)}d ago`;
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

const RunnerPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  const [runner, setRunner] = useState<RunnerDetails | null>(null);
  const [runnerLoading, setRunnerLoading] = useState(false);
  const [runnerError, setRunnerError] = useState(false);
  
  const online = runner ? runner.status === 'Online' : false;

  const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';
  const headerBg    = isDark ? '#1e1e1e' : '#fff';
  const bg          = isDark ? '#1a1a1a' : '#f5f5f5';
  const cardBg      = isDark ? '#242424' : '#fff';
  const dimColor    = isDark ? '#555'    : '#aaa';
  const onlineColor  = '#4caf50';
  const offlineColor = '#f44336';

  const loadRunner = async (showLoading = false) => {
    if (!id) return;
    if (showLoading) setRunnerLoading(true);
    setRunnerError(false);
    try {
      const data = await fetchRunner(id);
      setRunner(data);
    } catch {
      setRunnerError(true);
    } finally {
      if (showLoading) setRunnerLoading(false);
    }
  };

  useEffect(() => {
    loadRunner(true);
    const interval = setInterval(() => loadRunner(false), 5000);
    return () => clearInterval(interval);
  }, [id]);

  const sectionLabel = (text: string) => (
    <Typography sx={{
      ...MONO,
      fontSize: '0.62rem',
      color: dimColor,
      textTransform: 'uppercase',
      letterSpacing: '0.1em',
      mb: 1.5,
    }}>
      {text}
    </Typography>
  );

  const infoRow = (label: string, value: string) => (
    <Box sx={{ display: 'flex', gap: 2, mb: 0.75 }}>
      <Typography sx={{ ...MONO, fontSize: '0.75rem', color: dimColor, minWidth: 100 }}>{label}</Typography>
      <Typography sx={{ ...MONO, fontSize: '0.75rem', color: theme.palette.text.secondary }}>{value}</Typography>
    </Box>
  );

  if (runnerLoading) {
    return (
      <Box sx={{ p: 4 }}>
        <Typography sx={{ ...MONO, fontSize: '0.8rem', color: dimColor }}>Loading...</Typography>
      </Box>
    );
  }

  if (!runner) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        <Box sx={{
          display: 'flex',
          alignItems: 'center',
          px: 3, py: 1.5,
          borderBottom: `1px solid ${borderColor}`,
          background: headerBg,
          gap: 1,
          minHeight: 56,
          flexShrink: 0,
        }}>
          <Typography sx={{ ...MONO, fontSize: '1rem', fontWeight: 700, color: theme.palette.text.primary }}>
            Runner not found
          </Typography>
        </Box>
        <Box sx={{ p: 4 }}>
          <Typography sx={{ ...MONO, fontSize: '0.8rem', color: dimColor }}>
            Runner #{id} is no longer registered.
          </Typography>
        </Box>
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <Box sx={{
        display: 'flex',
        alignItems: 'center',
        px: 3, py: 1.5,
        borderBottom: `1px solid ${borderColor}`,
        background: headerBg,
        gap: 1.5,
        minHeight: 56,
        flexShrink: 0,
      }}>
        <Typography sx={{ ...MONO, fontSize: '1rem', fontWeight: 700, color: theme.palette.text.primary, flex: 1 }}>
          {runner.name}
        </Typography>
        <Tooltip title="Refresh">
          <IconButton size="small" onClick={() => loadRunner(true)} sx={{ color: theme.palette.text.secondary, '&:hover': { color: theme.palette.primary.main } }}>
            <RefreshIcon sx={{ fontSize: 18 }} />
          </IconButton>
        </Tooltip>
      </Box>

      <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto', p: 3, background: bg, display: 'flex', flexDirection: 'column', gap: 3 }}>

        <Box sx={{ background: cardBg, border: `1px solid ${borderColor}`, borderRadius: 1, p: 2.5 }}>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 2, mb: 1.5 }}>
            <Typography sx={{ ...MONO, fontSize: '0.62rem', color: dimColor, textTransform: 'uppercase', letterSpacing: '0.1em' }}>
              Details
            </Typography>
            <Chip
              label={online ? 'online' : 'offline'}
              size="small"
              sx={{
                ...MONO,
                fontSize: '0.7rem',
                fontWeight: 600,
                backgroundColor: online ? onlineColor : offlineColor,
                color: '#fff',
                flexShrink: 0,
              }}
            />
          </Box>
          {infoRow('id', runner.id)}
          {infoRow('last seen', formatRelativeTime(runner.lastSeenAt))}
          {infoRow('registered', formatDate(runner.createdAt))}
        </Box>

        <Box sx={{ background: cardBg, border: `1px solid ${borderColor}`, borderRadius: 1, p: 2.5 }}>
          {sectionLabel('Job types')}
          {runner.jobTypes.length === 0 ? (
            <Typography sx={{ ...MONO, fontSize: '0.75rem', color: dimColor }}>None</Typography>
          ) : (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
              {runner.jobTypes.map(jt => (
                <Chip
                  key={jt.fullName}
                  label={jt.fullName}
                  size="small"
                  sx={{
                    ...MONO,
                    fontSize: '0.68rem',
                    height: 22,
                    backgroundColor: isDark ? '#2e2e2e' : '#f0f0f0',
                    color: theme.palette.text.secondary,
                    borderRadius: '3px',
                    '& .MuiChip-label': { px: 1 },
                  }}
                />
              ))}
            </Box>
          )}
        </Box>

        <Box sx={{ background: cardBg, border: `1px solid ${borderColor}`, borderRadius: 1, p: 2.5 }}>
          {sectionLabel('Active jobs')}
          {runnerLoading && (
            <Typography sx={{ ...MONO, fontSize: '0.75rem', color: dimColor }}>Loading...</Typography>
          )}
          {!runnerLoading && runnerError && (
            <Typography sx={{ ...MONO, fontSize: '0.75rem', color: dimColor }}>
              Could not load active jobs.
            </Typography>
          )}
          {!runnerLoading && !runnerError && runner.jobs.length === 0 && (
            <Typography sx={{ ...MONO, fontSize: '0.75rem', color: dimColor }}>No active jobs.</Typography>
          )}
          {!runnerLoading && !runnerError && runner.jobs.length > 0 && (
            <Box sx={{
              border: `1px solid ${borderColor}`,
              borderRadius: '6px',
              overflow: 'hidden',
              fontFamily: "'IBM Plex Mono', monospace",
              fontSize: '0.78rem',
            }}>
              <Box sx={{
                display: 'grid',
                gridTemplateColumns: '80px 1fr 200px',
                background: isDark ? '#222' : '#fafafa',
                borderBottom: `1px solid ${borderColor}`,
              }}>
                {['ID', 'Type', 'Scheduled At'].map(h => (
                  <Box key={h} sx={{
                    px: 1.5, py: 0.75,
                    fontSize: '0.72rem',
                    fontFamily: "'IBM Plex Mono', monospace",
                    color: theme.palette.text.secondary,
                    textTransform: 'uppercase',
                    letterSpacing: '0.05em',
                  }}>
                    {h}
                  </Box>
                ))}
              </Box>
              {runner.jobs.map((job, idx) => (
                <Box
                  key={job.id}
                  onClick={() => navigate(`/jobs/${job.id}`)}
                  sx={{
                    display: 'grid',
                    gridTemplateColumns: '80px 1fr 200px',
                    cursor: 'pointer',
                    background: idx % 2 === 1 ? (isDark ? '#1e1e1e' : '#f9f9fc') : (isDark ? '#1a1a1a' : '#fff'),
                    borderBottom: idx < runner.jobs.length - 1 ? `1px solid ${borderColor}` : 'none',
                    '&:hover': { background: isDark ? '#2a2a2a' : '#f0f4ff' },
                  }}
                >
                  <Box sx={{ px: 1.5, py: 0.75, fontSize: '0.78rem', fontFamily: "'IBM Plex Mono', monospace", color: isDark ? '#ccc' : '#333' }}>
                    #{job.id}
                  </Box>
                  <Box sx={{ px: 1.5, py: 0.75, fontSize: '0.78rem', fontFamily: "'IBM Plex Mono', monospace", color: isDark ? '#ccc' : '#333' }} title={job.type.fullName}>
                    {job.type.name}
                  </Box>
                  <Box sx={{ px: 1.5, py: 0.75, fontSize: '0.78rem', fontFamily: "'IBM Plex Mono', monospace", color: isDark ? '#ccc' : '#333' }}>
                    {formatDate(job.scheduledAt)}
                  </Box>
                </Box>
              ))}
            </Box>
          )}
        </Box>

      </Box>
    </Box>
  );
};

export default RunnerPage;
