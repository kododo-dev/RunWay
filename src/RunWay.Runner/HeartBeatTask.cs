using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kododo.RunWay.Runner;

internal sealed class HeartBeatTask : BaseTask
{
    private readonly RunnerId _runnerId;
    private readonly TimeSpan _interval;
    private readonly TimeSpan _timeout;
    
    public HeartBeatTask(RunnerId runnerId, TimeSpan interval, TimeSpan timeout, IServiceProvider serviceProvider, ILogger logger)
        : base(serviceProvider, logger)
    {
        _interval = interval;
        _timeout = timeout;
        _runnerId = runnerId;
    }

    protected override async Task ExecuteAsync(AsyncServiceScope scope, IStore store, CancellationToken stoppingToken)
    {
        await store.KeepAliveAsync(_runnerId, DateTimeOffset.UtcNow.Add(_timeout), stoppingToken);
        await Task.Delay(_interval, stoppingToken);
    }
}