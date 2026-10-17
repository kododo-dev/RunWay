import React from 'react';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import RefreshIcon from '@mui/icons-material/Refresh';
import type { SxProps } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import { PageHeader as ShellPageHeader } from '@kododo-dev/way-shell';

interface PageHeaderProps {
  title: string;
  onRefresh?: () => void;
  actions?: React.ReactNode;
  sx?: SxProps;
}

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };

const PageHeader = ({ title, onRefresh, actions, sx }: PageHeaderProps) => {
  const theme = useTheme();

  // The shell's header bar: the menu button on narrow screens; the dashboard's title and actions in it.
  return (
    <Box sx={{ flexShrink: 0, ...sx }}>
      <ShellPageHeader title={<Box component="span" sx={MONO}>{title}</Box>}>
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
      </ShellPageHeader>
    </Box>
  );
};

export default PageHeader;
