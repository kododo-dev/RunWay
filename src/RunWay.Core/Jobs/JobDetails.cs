using Kododo.RunWay.Core.Jobs.Audit;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Runners;

namespace Kododo.RunWay.Core.Jobs;

public sealed record JobDetails(
    JobId Id,
    JobData Data,
    JobOptions Options,
    RecurrenceId? RecurrenceId,
    JobStatus Status,
    RunnerInfo? RunnerInfo,
    int RetriesCount,
    DateTimeOffset ModifiedAt,
    DateTimeOffset ScheduledAt,
    IReadOnlyCollection<JobAuditRecord> Audit);