using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Demo.WebApp.Jobs;

internal class ExportJob
{
    public required string Format { get; set; }
    public int TotalRecords { get; set; }
}

internal class ExportJobHandler : IJobHandler<ExportJob>
{
    public async Task HandleAsync(ExportJob data, CancellationToken stoppingToken)
    {
        Console.WriteLine($"[ExportJob] Exporting {data.TotalRecords} records to {data.Format}...");
        for (int i = 1; i <= 12; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            Console.WriteLine($"[ExportJob] Exported chunk {i}/12");
        }
        Console.WriteLine($"[ExportJob] Export to {data.Format} complete.");
    }
}