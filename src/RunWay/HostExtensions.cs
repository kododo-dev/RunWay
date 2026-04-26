using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Schedule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace Kododo.RunWay;

public static class HostExtensions
{
    public static async Task InitializeRunWayDatabaseAsync(this IHost host, CancellationToken stoppingToken = default)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStore>();
        await store.InitializeAsync(stoppingToken);
    }

    public static async Task SetRecurrenceAsync<T>(this IHost host, string key, string cronExpression, T data, Action<IJobBuilder<T>> configure, CancellationToken stoppingToken = default)
    {
        CrontabSchedule.Parse(cronExpression);
        await using var scope = host.Services.CreateAsyncScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
        var builder = scheduler.Recurrence(data);
        configure(builder);
        await builder.SetRecurrenceAsync(key, cronExpression, stoppingToken);
    }

    public static async Task SetRecurrenceAsync<T>(this IHost host, string key, string cronExpression, T data, CancellationToken stoppingToken = default)
    {
        await host.SetRecurrenceAsync(key, cronExpression, data, _ => { }, stoppingToken);
    }
}