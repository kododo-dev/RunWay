using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Demo.WebApp.Jobs;

internal class ReportJob
{
    public required string ReportName { get; set; }
    public int Rows { get; set; }
}

internal class ReportJobHandler : IJobHandler<ReportJob>
{
    public async Task HandleAsync(ReportJob data, CancellationToken stoppingToken)
    {
        Console.WriteLine($"[ReportJob] Generating '{data.ReportName}' ({data.Rows} rows)...");
        for (int i = 1; i <= 3; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            Console.WriteLine($"[ReportJob] Progress: {i * 33}%");
        }
        Console.WriteLine($"[ReportJob] Report '{data.ReportName}' complete.");
    }
}