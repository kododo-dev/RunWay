namespace Kododo.RunWay.Schedule;

public interface IJobBuilder<T>
{
    IJobBuilder<T> AsTransactional(bool transactional);
    IJobBuilder<T> WithPriority(int priority);
    IJobBuilder<T> WithRetryDelaysInSeconds(params int[] retryDelaysInSeconds);
    IJobBuilder<T> WithTimeout(TimeSpan timeout);
}

public abstract class JobBuilder<T>(RunWayOptions options) : IJobBuilder<T>
{
    protected readonly JobOptionsBuilder JobOptionsBuilder = new(options.SchedulerOptions);
    protected bool Transactional = options.SchedulerOptions.TransactionalDefault;

    public IJobBuilder<T> AsTransactional(bool transactional)
    {
        Transactional = transactional;
        return this;
    }

    public IJobBuilder<T> WithPriority(int priority)
    {
        JobOptionsBuilder.Priority = priority;
        return this;
    }
    
    public IJobBuilder<T> WithRetryDelaysInSeconds(params int[] retryDelaysInSeconds)
    {
        JobOptionsBuilder.RetryDelaysInSeconds = retryDelaysInSeconds;
        return this;
    }

    public IJobBuilder<T> WithTimeout(TimeSpan timeout)
    {
        JobOptionsBuilder.Timeout = timeout;
        return this;
    }
}