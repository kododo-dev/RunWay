import Chip from '@mui/material/Chip';
import { type JobStatus } from '../../api/api.model';
import { statusName, STATUS_COLORS } from '../../utils/jobUtils';

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };

interface JobStatusBadgeProps {
    status: JobStatus;
}

const JobStatusBadge = ({ status }: JobStatusBadgeProps) => (
    <Chip
        label={statusName(status)}
        size="small"
        sx={{
            ...MONO,
            fontSize: '0.68rem',
            fontWeight: 600,
            height: 20,
            backgroundColor: STATUS_COLORS[status] ?? '#888',
            color: '#fff',
        }}
    />
);

export default JobStatusBadge;
