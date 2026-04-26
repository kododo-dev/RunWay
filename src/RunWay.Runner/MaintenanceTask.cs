using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kododo.RunWay.Runner;

internal sealed class MaintenanceTask : BaseTask
{
    private readonly TimeSpan _interval;
    private readonly TimeSpan? _deleteSucceededAfterTimeSpan;
    private readonly ILogger _logger;
    
    public MaintenanceTask(TimeSpan interval, TimeSpan? deleteSucceededAfterTimeSpan, IServiceProvider serviceProvider, ILogger logger)
        : base(serviceProvider, logger)
    {
        _interval = interval;
        _deleteSucceededAfterTimeSpan = deleteSucceededAfterTimeSpan;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(AsyncServiceScope scope, IStore store, CancellationToken stoppingToken)
    {
        try
        {
            while((await store.GetAllRunnersAsync(stoppingToken)).FirstOrDefault(x => x.Status == RunnerStatus.Offline) is { } runner)
            {
                var query = new JobsQuery
                {
                    Filter = new JobsFilter
                    {
                        AssignedRunnerId = runner.Id
                    },
                    Pagination = new Pagination(1)
                };

                PagedResult<Job> jobs;
                while ((jobs = await store.GetManyAsync(query, stoppingToken)).Items.Count > 0)
                {
                    foreach (var job in jobs.Items)
                    {
                        job.Failed("Runner was lost while job was running.");
                        var completedJob = await store.UpdateAsync(job, stoppingToken);
                        await OnCompletedAsync(completedJob, scope, store, stoppingToken);
                    }
                }
                
                await store.RemoveRunnerAsync(runner.Id, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            if(!store.IsConcurrencyException(ex))
            {
                _logger.LogError(ex, "An error occurred while executing the maintenance loop of the job runner.");
            }
        }

        if (_deleteSucceededAfterTimeSpan.HasValue)
        {
            try
            {
                var filter = new JobsFilter()
                {
                    Statuses = [JobStatus.Succeeded],
                    MaxModifiedAt = DateTimeOffset.UtcNow.Subtract(_deleteSucceededAfterTimeSpan.Value)
                };
                        
                await store.DeleteAsync(filter, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting old jobs in the maintenance loop of the job runner.");
            }
        }
                
        await Task.Delay(_interval, stoppingToken);
    }
}