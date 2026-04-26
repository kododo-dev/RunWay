using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Core.Runners;

public sealed class RunnerDetails(
    RunnerId id,
    string name,
    DateTimeOffset createdAt,
    DateTimeOffset lastSeenAt,
    IReadOnlyCollection<string> jobTypes,
    IReadOnlyCollection<Job> jobs,
    DateTimeOffset expiringAt)
    : Runner(id, name, lastSeenAt, expiringAt)
{
    public DateTimeOffset CreatedAt { get; } = createdAt;
    public IReadOnlyCollection<string> JobTypes { get; } = jobTypes;
    public IReadOnlyCollection<Job> Jobs { get; } = jobs;
}