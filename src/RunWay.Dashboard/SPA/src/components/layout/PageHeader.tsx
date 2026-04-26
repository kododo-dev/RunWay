import React from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import RefreshIcon from '@mui/icons-material/Refresh';
import MenuIcon from '@mui/icons-material/Menu';
import useMediaQuery from '@mui/material/useMediaQuery';
import type { SxProps } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import { useLayout } from '../../context/LayoutContext';

interface PageHeaderProps {
  title: string;
  onRefresh?: () => void;
  actions?: React.ReactNode;
  sx?: SxProps;
}

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };

const PageHeader = ({ title, onRefresh, actions, sx }: PageHeaderProps) => {
  const theme = useTheme();
  const { openMobileMenu } = useLayout();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const isDark = theme.palette.mode === 'dark';

  const borderColor = isDark ? '#2e2e2e' : '#e0e0e0';
  const headerBg    = isDark ? '#1e1e1e' : '#fff';

  return (
    <Box sx={{ flexShrink: 0, ...sx }}>
      <Box sx={{
        display: 'flex',
        alignItems: 'center',
        px: 3,
        py: 1.5,
        borderBottom: `1px solid ${borderColor}`,
        background: headerBg,
        gap: 1.5,
        minHeight: 59,
      }}>
        {isMobile && (
          <IconButton
            size="small"
            onClick={openMobileMenu}
            sx={{ color: theme.palette.text.secondary, flexShrink: 0, mr: 0.5, '&:hover': { color: theme.palette.primary.main } }}
          >
            <MenuIcon sx={{ fontSize: 20 }} />
          </IconButton>
        )}

        <Box sx={{ minWidth: 0, flex: 1 }}>
          <Typography sx={{
            ...MONO,
            fontSize: '1rem',
            fontWeight: 700,
            color: theme.palette.text.primary,
            lineHeight: 1.2,
            whiteSpace: 'nowrap',
          }}>
            {title}
          </Typography>
        </Box>

        {actions}

        {onRefresh && (
          <Tooltip title="Refresh">
            <IconButton
              size="small"
              onClick={onRefresh}
              sx={{ color: theme.palette.text.secondary, flexShrink: 0, '&:hover': { color: theme.palette.primary.main } }}
            >
              <RefreshIcon sx={{ fontSize: 18 }} />
            </IconButton>
          </Tooltip>
        )}
      </Box>
    </Box>
  );
};

export default PageHeader;
