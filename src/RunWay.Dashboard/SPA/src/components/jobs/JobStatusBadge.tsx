import Chip from '@mui/material/Chip';
import { type JobStatus } from '../../api/api.model';
import { statusName, STATUS_COLORS } from '../../utils/jobUtils';
import { useI18n } from '../../i18n/i18n';

const MONO = { fontFamily: "'IBM Plex Mono', monospace" };

interface JobStatusBadgeProps {
    status: JobStatus;
}

const JobStatusBadge = ({ status }: JobStatusBadgeProps) => {
    const { t } = useI18n();
    return (
    <Chip
        label={statusName(status, t)}
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
};

export default JobStatusBadge;
