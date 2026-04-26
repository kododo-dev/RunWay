using Kododo.RunWay.Core.Handlers;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kododo.RunWay.Runner;

internal sealed class JobsRunnerTask : BaseTask
{
    private readonly SemaphoreSlim _semaphore;
    private readonly TimeSpan _interval;
    private readonly RunnerInfo _runnerInfo;

    public JobsRunnerTask(SemaphoreSlim semaphore, TimeSpan interval, RunnerInfo runnerInfo, IServiceProvider serviceProvider, ILogger logger)
        : base(serviceProvider, logger)
    {
        _semaphore = semaphore;
        _interval = interval;
        _runnerInfo = runnerInfo;
    }

    protected override async Task ExecuteAsync(AsyncServiceScope scope, IStore store, CancellationToken stoppingToken)
    {
        var query = new JobQuery
        {
            Filter = new JobsFilter
            {
                Statuses = [JobStatus.Scheduled],
                MaxScheduledAt = DateTimeOffset.UtcNow,
                SuitableForRunnerId = _runnerInfo.Id
            },
            Sorting =
            [
                new JobsSorting(JobSortingField.Priority, false),
                new JobsSorting(JobSortingField.ScheduledAt)
            ]
        };
            
        var needsDelay = true;
        
        Job? job;
        await _semaphore.WaitAsync(stoppingToken);
        try
        {
            job = await store.GetOneOrDefaultAsync(query, stoppingToken);
        }
        finally
        {
            _semaphore.Release();
        }

        if (job != null)
        {
            needsDelay = false;

            try
            {
                var runWayOptions = scope.ServiceProvider.GetRequiredService<RunWayOptions>();
                var jobDataSerializer = runWayOptions.JobDataSerializer;
                var jobData = jobDataSerializer.Deserialize(job.Data);
                var handlerType = typeof(IJobHandler<>).MakeGenericType(jobData.GetType());
                var handler = (IJobHandler)scope.ServiceProvider.GetRequiredService(handlerType);

                job.Started(_runnerInfo);
                job = await store.UpdateAsync(job, stoppingToken);

                using var cts = BuildTimeoutCts(job.Options.Timeout, stoppingToken);
                await handler.HandleAsync(jobData, cts.Token);
                
                job.Succeeded();
                job = await store.UpdateAsync(job, stoppingToken);
                await OnCompletedAsync(job, scope, store, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                job.Failed(stoppingToken.IsCancellationRequested
                    ? "Job was cancelled due to runner shutdown."
                    : $"Job timed out after {job.Options.Timeout}.");

                job = await store.UpdateAsync(job, stoppingToken);
                await OnCompletedAsync(job, scope, store, stoppingToken);
            }
            catch (Exception ex)
            {
                if (!store.IsConcurrencyException(ex))
                {
                    job.Failed(ex.ToString());
                    job = await store.UpdateAsync(job, stoppingToken);
                    await OnCompletedAsync(job, scope, store, stoppingToken);
                }
            }
        }

        if (needsDelay)
        {
            await Task.Delay(_interval, stoppingToken);
        }
    }
    
    private static CancellationTokenSource BuildTimeoutCts(TimeSpan timeout, CancellationToken stoppingToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(timeout);
        return cts;
    }
}