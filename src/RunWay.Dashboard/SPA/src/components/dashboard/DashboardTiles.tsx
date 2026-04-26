import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { useTheme } from '@mui/material/styles';
import { useNavigate } from 'react-router-dom';
import { CATEGORY_COLORS } from '../../utils/jobUtils';

interface DashboardTilesProps {
  jobCounts: {
    running: number;
    succeeded: number;
    failed: number;
    pending: number;
    retrying: number;
  };
}

const tiles = [
  { key: 'pending',   label: 'Pending',   color: CATEGORY_COLORS.pending },
  { key: 'running',   label: 'Running',   color: CATEGORY_COLORS.running },
  { key: 'retrying',  label: 'Retrying',  color: CATEGORY_COLORS.retrying },
  { key: 'succeeded', label: 'Succeeded', color: CATEGORY_COLORS.succeeded },
  { key: 'failed',    label: 'Failed',    color: CATEGORY_COLORS.failed },
];

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };

const DashboardTiles = ({ jobCounts }: DashboardTilesProps) => {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const navigate = useNavigate();

  const cardBg     = isDark ? '#222' : '#fff';
  const borderBase = isDark ? '#2e2e2e' : '#e0e0e0';

  return (
    <Box sx={{
      display: 'flex',
      gap: 2,
      p: 3,
      flexWrap: 'wrap',
    }}>
      {tiles.map(tile => {
        const count = jobCounts[tile.key as keyof typeof jobCounts] ?? 0;
        return (
          <Box
            key={tile.key}
            onClick={() => navigate(`/jobs/category/${tile.key}`)}
            sx={{
              flex: '1 1 140px',
              minWidth: 120,
              background: cardBg,
              border: `1px solid ${borderBase}`,
              borderTop: `3px solid ${tile.color}`,
              borderRadius: '6px',
              p: 2,
              cursor: 'pointer',
              transition: 'border-color 0.15s, background 0.15s',
              '&:hover': {
                borderColor: tile.color,
                background: isDark ? '#2a2a2a' : '#fafafa',
              },
            }}
          >
            <Typography sx={{
              ...MONO,
              fontSize: '1.8rem',
              fontWeight: 700,
              color: tile.color,
              lineHeight: 1,
            }}>
              {count}
            </Typography>
            <Typography sx={{
              ...MONO,
              fontSize: '0.72rem',
              color: theme.palette.text.secondary,
              mt: 0.5,
              textTransform: 'uppercase',
              letterSpacing: '0.06em',
            }}>
              {tile.label}
            </Typography>
          </Box>
        );
      })}
    </Box>
  );
};

export default DashboardTiles;
