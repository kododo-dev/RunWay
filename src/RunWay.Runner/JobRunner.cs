using Kododo.RunWay.Core.Handlers;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kododo.RunWay.Runner;

internal sealed class JobRunner(IOptions<JobRunnerOptions> options, ILogger<JobRunner> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly JobRunnerOptions _options = options.Value;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runnerInfo = await InitializeAsync(stoppingToken);
        
        var tasks = Enumerable.Range(0, _options.ThreadsCount)
            .Select(_ => Task.Run(() => new JobsRunnerTask(_semaphore, _options.Interval, runnerInfo, serviceProvider, logger).Run(stoppingToken), stoppingToken))
            .ToList();
        
        tasks.Add(Task.Run(() => new HeartBeatTask(runnerInfo.Id, _options.HeartbeatInterval, _options.HeartbeatTimeout, serviceProvider, logger).Run(stoppingToken), stoppingToken));
        tasks.Add(Task.Run(() => new MaintenanceTask(_options.MaintenanceInterval, _options.DeleteSucceededAfterTimeSpan, serviceProvider, logger).Run(stoppingToken), stoppingToken));
        
        await Task.WhenAll(tasks);
        await CleanupAsync(runnerInfo);
    }
    
    public override void Dispose()
    {
        base.Dispose();
        _semaphore.Dispose();
    }

    private async Task<RunnerInfo> InitializeAsync(CancellationToken stoppingToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStore>();
        
        var runWayOptions = scope.ServiceProvider.GetRequiredService<RunWayOptions>();
        var jobTypes = _options.HandlerTypes
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobHandler<>))
                .Select(i => i.GetGenericArguments()[0]))
            .Select(t => runWayOptions.JobDataSerializer.SerializeType(t))
            .ToArray();

        var expiresAt = DateTimeOffset.UtcNow.Add(_options.HeartbeatTimeout);
        var runnerId = await store.RegisterRunnerAsync(_options.Name, jobTypes, expiresAt, stoppingToken);
        return new RunnerInfo(runnerId, _options.Name);
    }
    
    private async Task CleanupAsync(RunnerInfo runnerInfo)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStore>();
        await store.RemoveRunnerAsync(runnerInfo.Id, CancellationToken.None);
    }
}