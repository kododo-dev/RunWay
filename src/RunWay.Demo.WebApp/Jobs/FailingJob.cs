using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Demo.WebApp.Jobs;

internal class FailingJob
{
    public required string Reason { get; set; }
}

internal class FailingJobHandler : IJobHandler<FailingJob>
{
    public async Task HandleAsync(FailingJob data, CancellationToken stoppingToken)
    {
        Console.WriteLine($"[FailingJob] Starting, will fail because: {data.Reason}");
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        throw new InvalidOperationException($"Intentional failure: {data.Reason}");
    }
}
