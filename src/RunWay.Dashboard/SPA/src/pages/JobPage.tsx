import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { fetchJobDetails, requeueJob } from '../api/api';
import type { JobDetails, JobAuditItem } from '../api/api.model';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import Divider from '@mui/material/Divider';
import Chip from '@mui/material/Chip';
import Button from '@mui/material/Button';
import ReplayIcon from '@mui/icons-material/Replay';
import {
  Timeline,
  TimelineItem,
  TimelineSeparator,
  TimelineDot,
  TimelineConnector,
  TimelineContent,
  TimelineOppositeContent,
  timelineOppositeContentClasses,
} from '@mui/lab';
import { useTheme } from '@mui/material/styles';
import PageHeader from '../components/layout/PageHeader';
import { statusName, auditTypeName, auditDotColor, formatTimeDelta, localDate, STATUS_COLORS } from '../utils/jobUtils';

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };
const REFRESH_INTERVAL = 5000;

const JobPage = () => {
  const { id } = useParams<{ id: string }>();
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  const [job, setJob] = useState<JobDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [requeueing, setRequeueing] = useState(false);
  const [requeueError, setRequeueError] = useState<string | null>(null);

  const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';
  const cardBg      = isDark ? '#222'    : '#fff';
  const sectionBg   = isDark ? '#1e1e1e' : '#fafafa';

  const fetchAndSetJob = useCallback(() => {
    if (!id) return;
    fetchJobDetails(id)
      .then(data => { setJob(data); setError(null); })
      .catch(() => setError('Error while fetching job details.'))
      .finally(() => setLoading(false));
  }, [id]);

  useEffect(() => {
    setLoading(true);
    fetchAndSetJob();
    const interval = setInterval(fetchAndSetJob, REFRESH_INTERVAL);
    return () => clearInterval(interval);
  }, [fetchAndSetJob]);

  const handleRequeue = async () => {
    if (!id) return;
    setRequeueing(true);
    setRequeueError(null);
    try {
      await requeueJob(id);
      fetchAndSetJob();
    } catch {
      setRequeueError('Requeue failed. Please try again.');
    } finally {
      setRequeueing(false);
    }
  };

  const requeueAction = job?.status === 'Failed' ? (
    <Button
      size="small"
      variant="outlined"
      color="warning"
      startIcon={requeueing ? <CircularProgress size={12} color="inherit" /> : <ReplayIcon sx={{ fontSize: 14 }} />}
      disabled={requeueing}
      onClick={handleRequeue}
      sx={{ ...MONO, fontSize: '0.7rem', textTransform: 'none', py: 0.25 }}
    >
      Requeue
    </Button>
  ) : null;

  const renderContent = () => {
    if (loading) return (
      <Box sx={{ display: 'flex', justifyContent: 'center', pt: 6 }}>
        <CircularProgress size={24} />
      </Box>
    );
    if (error) return <Alert severity="error" sx={{ m: 3, ...MONO, fontSize: '0.78rem' }}>{error}</Alert>;
    if (!job)  return <Alert severity="warning" sx={{ m: 3, ...MONO, fontSize: '0.78rem' }}>Job not found.</Alert>;

    let parsedData: unknown;
    try { parsedData = job.data ? JSON.parse(job.data) : null; }
    catch { parsedData = job.data; }

    return (
      <Box sx={{ p: 3, display: 'flex', flexDirection: 'column', gap: 3 }}>
        {requeueError && (
          <Alert severity="error" onClose={() => setRequeueError(null)} sx={{ ...MONO, fontSize: '0.78rem' }}>
            {requeueError}
          </Alert>
        )}
        <Box sx={{
          background: cardBg,
          border: `1px solid ${borderColor}`,
          borderRadius: '6px',
          p: 2.5,
        }}>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 2, mb: 1 }}>
            <Box>
              <Typography sx={{ ...MONO, fontSize: '1rem', fontWeight: 700, color: theme.palette.text.primary }}>
                #{job.id} {job.type.name}
              </Typography>
              <Typography sx={{ ...MONO, fontSize: '0.72rem', color: theme.palette.text.secondary, mt: 0.5 }}>
                {job.type.fullName}
              </Typography>
            </Box>
            <Chip
              label={statusName(job.status)}
              size="small"
              sx={{
                ...MONO,
                fontSize: '0.7rem',
                fontWeight: 600,
                backgroundColor: STATUS_COLORS[job.status],
                color: '#fff',
                flexShrink: 0,
              }}
            />
          </Box>
          <Divider sx={{ borderColor, my: 1.5 }} />
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            {job.priority !== 0 && (
              <Box>
                <Typography sx={{ ...MONO, fontSize: '0.62rem', color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: '0.06em' }}>Priority</Typography>
                <Typography sx={{ ...MONO, fontSize: '0.85rem', color: theme.palette.text.primary }}>{job.priority}</Typography>
              </Box>
            )}
            {job.retriesCount !== 0 && (
              <Box>
                <Typography sx={{ ...MONO, fontSize: '0.62rem', color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: '0.06em' }}>Retries</Typography>
                <Typography sx={{ ...MONO, fontSize: '0.85rem', color: theme.palette.text.primary }}>{job.retriesCount}</Typography>
              </Box>
            )}
          </Box>
        </Box>

        <Box>
          <Typography sx={{ ...MONO, fontSize: '0.72rem', color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: '0.08em', mb: 1 }}>
            Payload
          </Typography>
          <Box sx={{
            background: sectionBg,
            border: `1px solid ${borderColor}`,
            borderRadius: '6px',
            p: 2,
          }}>
            <Box
              component="pre"
              sx={{
                m: 0,
                fontSize: '0.8rem',
                fontFamily: "'IBM Plex Mono', 'Consolas', monospace",
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word',
              }}
            >
              {JSON.stringify(parsedData ?? {}, null, 2)}
            </Box>
          </Box>
        </Box>

        <Box>
          <Typography sx={{ ...MONO, fontSize: '0.72rem', color: theme.palette.text.secondary, textTransform: 'uppercase', letterSpacing: '0.08em', mb: 1 }}>
            Audit timeline
          </Typography>
          <Box sx={{
            background: cardBg,
            border: `1px solid ${borderColor}`,
            borderRadius: '6px',
            p: 1,
          }}>
            {job.audit && job.audit.length > 0 ? (
              <Timeline sx={{
                [`& .${timelineOppositeContentClasses.root}`]: { flex: 0.25 },
                my: 0,
              }}>
                {job.audit.map((item: JobAuditItem, idx: number) => {
                  const t1 = new Date(item.time).getTime();
                  const delta = idx < job.audit.length - 1
                    ? formatTimeDelta(t1 - new Date(job.audit[idx + 1].time).getTime())
                    : '';
                  const deltaFromStart = job.audit.length > 1
                    ? formatTimeDelta(t1 - new Date(job.audit[job.audit.length - 1].time).getTime())
                    : '';
                  return (
                    <TimelineItem key={idx}>
                      <TimelineOppositeContent sx={{ ...MONO, fontSize: '0.7rem', color: theme.palette.text.secondary, pt: 1.5 }}>
                        {localDate(item.time)}
                        {delta && (
                          <Box sx={{ ...MONO, fontSize: '0.65rem', color: theme.palette.text.disabled, mt: 0.3 }}>
                            {delta}
                            {deltaFromStart && (
                              <Box component="span" sx={{ color: theme.palette.text.disabled, ml: 0.5 }}>
                                ({deltaFromStart})
                              </Box>
                            )}
                          </Box>
                        )}
                      </TimelineOppositeContent>
                      <TimelineSeparator>
                        <TimelineDot color={auditDotColor(item.type)} />
                        {idx < job.audit.length - 1 && <TimelineConnector />}
                      </TimelineSeparator>
                      <TimelineContent sx={{ pb: 2, pt: 1.5 }}>
                        <Typography sx={{ ...MONO, fontSize: '0.82rem', fontWeight: 600, color: theme.palette.text.primary }}>
                          {auditTypeName(item.type)}
                        </Typography>
                        {item.details && (
                          <Typography sx={{ ...MONO, fontSize: '0.72rem', color: theme.palette.text.secondary, mt: 0.3, whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
                            {item.details}
                          </Typography>
                        )}
                      </TimelineContent>
                    </TimelineItem>
                  );
                })}
              </Timeline>
            ) : (
              <Typography sx={{ ...MONO, fontSize: '0.78rem', color: theme.palette.text.secondary, p: 2 }}>
                No audit records.
              </Typography>
            )}
          </Box>
        </Box>
      </Box>
    );
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <PageHeader title={`Job #${id}`} actions={requeueAction} onRefresh={fetchAndSetJob} />
      <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto' }}>
        {renderContent()}
      </Box>
    </Box>
  );
};

export default JobPage;
