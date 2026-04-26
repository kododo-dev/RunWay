using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Demo.WebApp.Jobs;

internal class DataSyncJob
{
    public required string Source { get; set; }
    public required string Destination { get; set; }
    public int RecordCount { get; set; }
}

internal class DataSyncJobHandler : IJobHandler<DataSyncJob>
{
    public async Task HandleAsync(DataSyncJob data, CancellationToken stoppingToken)
    {
        Console.WriteLine($"[DataSyncJob] Syncing {data.RecordCount} records: {data.Source} → {data.Destination}");
        for (int i = 1; i <= 6; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            Console.WriteLine($"[DataSyncJob] Synced batch {i}/6");
        }
        Console.WriteLine("[DataSyncJob] Sync complete.");
    }
}