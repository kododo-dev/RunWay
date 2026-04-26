using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Dashboard.API;

internal sealed record JobListItem(
    string Id,
    JobType Type,
    JobStatus Status,
    int Priority,
    int RetriesCount,
    DateTime ScheduledAt,
    DateTime ModifiedAt)
{
    public static JobListItem FromJob(Job job)
    {
        return new JobListItem(
            job.Id.Value,
            JobType.FromFullTypeName(job.Data.Type),
            job.Status,
            job.Options.Priority,
            job.RetriesCount,
            job.ScheduledAt.UtcDateTime,
            job.ModifiedAt.UtcDateTime);
    }
}