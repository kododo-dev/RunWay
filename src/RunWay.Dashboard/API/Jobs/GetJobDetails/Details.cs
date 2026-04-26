using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetJobDetails;

internal sealed record Details(
    string Id,
    JobType Type,
    string Data,
    JobStatus Status,
    int Priority,
    int RetriesCount,
    DateTime ScheduledAt,
    DateTime ModifiedAt,
    AuditItem[] Audit);