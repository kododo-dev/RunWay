using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Schedule;

public class JobOptionsBuilder(SchedulerOptions options)
{
    public int Priority { get; set; } = options.Priority;
    public int[] RetryDelaysInSeconds { get; set; } = options.RetryDelaysInSeconds;
    public TimeSpan Timeout { get; set; } = options.Timeout;

    internal JobOptions Build()
    {
        return new JobOptions(Priority, RetryDelaysInSeconds, Timeout);
    }
}