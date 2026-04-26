using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Demo.WebApp.Jobs;

internal class QuickJob
{
    public required string TaskName { get; set; }
}

internal class QuickJobHandler : IJobHandler<QuickJob>
{
    public async Task HandleAsync(QuickJob data, CancellationToken stoppingToken)
    {
        Console.WriteLine($"[QuickJob] Starting: {data.TaskName}");
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        Console.WriteLine($"[QuickJob] Done: {data.TaskName}");
    }
}