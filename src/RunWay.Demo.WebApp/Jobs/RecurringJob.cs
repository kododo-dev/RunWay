using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Demo.WebApp.Jobs;

internal class RecurringJob
{
    public required string TaskName { get; set; }
}

internal class RecurringJobHandler : IJobHandler<RecurringJob>
{
    public async Task HandleAsync(RecurringJob data, CancellationToken stoppingToken)
    {
        Console.WriteLine($"[RecurringJob] Starting: {data.TaskName}");
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        Console.WriteLine($"[RecurringJob] Done: {data.TaskName}");
    }
}