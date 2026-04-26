namespace Kododo.RunWay.Core.Runners;

public  class Runner(RunnerId id, string name, DateTimeOffset lastSeenAt, DateTimeOffset expiresAt)
{
    public RunnerId Id { get; } = id;
    public string Name { get; } = name;
    public DateTimeOffset LastSeenAt { get; } = lastSeenAt;
    public RunnerStatus Status { get; } =
        DateTimeOffset.UtcNow > expiresAt
            ? RunnerStatus.Offline
            : RunnerStatus.Online;
}