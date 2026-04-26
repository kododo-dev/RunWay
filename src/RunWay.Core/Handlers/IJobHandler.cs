namespace Kododo.RunWay.Core.Handlers;

public interface IJobHandler
{
    Type JobType { get; }
    Task HandleAsync(object data, CancellationToken stoppingToken);
}


public interface IJobHandler<in T> : IJobHandler
{
    Type IJobHandler.JobType => typeof(T);

    async Task IJobHandler.HandleAsync(object data, CancellationToken stoppingToken)
    {
        if (data is not T jobData)
        {
            throw new ArgumentException($"Job data must be of type {typeof(T).Name}");
        }
        
        await HandleAsync(jobData, stoppingToken);
    }

    Task HandleAsync(T data, CancellationToken stoppingToken);
}