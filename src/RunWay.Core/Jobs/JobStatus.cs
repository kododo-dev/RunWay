namespace Kododo.RunWay.Core.Jobs;

public enum JobStatus
{
    Scheduled,
    Running,
    Succeeded,
    Failed,
}

public static class JobStatusExtensions
{
    public static bool IsCompleted(this JobStatus status) =>
        status is JobStatus.Succeeded or JobStatus.Failed;
}