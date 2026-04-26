using Kododo.RunWay.Core.Runners;

namespace Kododo.RunWay.Dashboard.API.Runners.GetRunners;

internal sealed record RunnerListItem(
    string Id,
    string Name,
    RunnerStatus Status,
    DateTime LastSeenAt);