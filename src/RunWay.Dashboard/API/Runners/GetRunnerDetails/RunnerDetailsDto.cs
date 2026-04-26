using Kododo.RunWay.Core.Runners;

namespace Kododo.RunWay.Dashboard.API.Runners.GetRunnerDetails;

internal sealed record RunnerDetailsDto(
    string Id,
    string Name,
    RunnerStatus Status,
    DateTime CreatedAt,
    DateTime LastSeenAt,
    IReadOnlyCollection<JobType> JobTypes,
    IReadOnlyCollection<JobListItem> Jobs);