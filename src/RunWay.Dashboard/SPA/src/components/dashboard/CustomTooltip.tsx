import type { TooltipContentProps } from 'recharts';
import type { ValueType, NameType } from 'recharts/types/component/DefaultTooltipContent';
import { useTheme } from '@mui/material/styles';

const CustomTooltip = ({ active, payload, label }: TooltipContentProps<ValueType, NameType>) => {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  if (!active || !payload || !payload.length) return null;

  return (
    <div style={{
      background: isDark ? '#2a2a2a' : '#fff',
      border: `1px solid ${isDark ? '#3a3a3a' : '#ddd'}`,
      padding: '8px 12px',
      borderRadius: 4,
      fontSize: 12,
      fontFamily: "'IBM Plex Mono', monospace",
      color: isDark ? '#e0e0e0' : '#1a1a1a',
    }}>
      <div style={{ marginBottom: 4, color: isDark ? '#888' : '#999' }}>{label}</div>
      {payload.map(entry => (
        <div key={entry.dataKey?.toString()} style={{ color: entry.stroke as string }}>
          {entry.name}: <strong>{entry.value}</strong>
        </div>
      ))}
    </div>
  );
};

export default CustomTooltip;
