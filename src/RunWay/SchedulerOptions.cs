using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay;

public class SchedulerOptions
{
    public bool TransactionalDefault { get; set; } = true;
    public int Priority { get; set; }  = JobOptions.Default.Priority;
    public int[] RetryDelaysInSeconds { get; set; } = JobOptions.Default.RetryDelaysInSeconds.ToArray();
    public TimeSpan Timeout { get; set; } = JobOptions.Default.Timeout;
}