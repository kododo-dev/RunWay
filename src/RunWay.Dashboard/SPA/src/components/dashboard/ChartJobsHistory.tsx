import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import type { JobMetricsHistoryItem } from '../../context/JobMetricsContext';
import CustomTooltip from './CustomTooltip';
import React from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { useTheme } from '@mui/material/styles';
import { CATEGORY_COLORS } from '../../utils/jobUtils';

interface ChartJobsHistoryProps {
  history: JobMetricsHistoryItem[];
}

const ChartJobsHistory: React.FC<ChartJobsHistoryProps> = ({ history }) => {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  const gridColor  = isDark ? '#2a2a2a' : '#eee';
  const axisColor  = isDark ? '#555'    : '#ccc';

  const chartData = history.map(item => ({
    time: new Date(item.timestamp).toLocaleTimeString(),
    pending:  item.pending,
    running:  item.running,
    retrying: item.retrying,
  }));

  return (
    <Box sx={{ px: 3, pb: 3 }}>
      <Typography sx={{
        fontFamily: "'IBM Plex Mono', monospace",
        fontSize: '0.72rem',
        color: theme.palette.text.secondary,
        textTransform: 'uppercase',
        letterSpacing: '0.08em',
        mb: 1.5,
      }}>
        Active jobs history
      </Typography>
      <Box sx={{
        background: isDark ? '#222' : '#fff',
        border: `1px solid ${isDark ? '#2e2e2e' : '#e0e0e0'}`,
        borderRadius: '6px',
        p: 2,
      }}>
        <ResponsiveContainer width="100%" height={240}>
          <LineChart data={chartData} margin={{ top: 8, right: 16, left: -16, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke={gridColor} />
            <XAxis dataKey="time" minTickGap={20} tick={false} axisLine={{ stroke: axisColor }} tickLine={false} />
            <YAxis allowDecimals={false} tick={{ fontSize: 11, fontFamily: 'monospace', fill: axisColor }} axisLine={false} tickLine={false} />
            <Tooltip content={CustomTooltip} />
            <Line type="monotone" dataKey="pending"  stroke={CATEGORY_COLORS.pending}  dot={false} name="Pending"  strokeWidth={1.5} />
            <Line type="monotone" dataKey="running"  stroke={CATEGORY_COLORS.running}  dot={false} name="Running"  strokeWidth={1.5} />
            <Line type="monotone" dataKey="retrying" stroke={CATEGORY_COLORS.retrying} dot={false} name="Retrying" strokeWidth={1.5} />
          </LineChart>
        </ResponsiveContainer>
      </Box>
    </Box>
  );
};

export default ChartJobsHistory;
