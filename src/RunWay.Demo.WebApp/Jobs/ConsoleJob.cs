using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Demo.WebApp.Jobs;

internal class ConsoleJob
{
    public required string Text { get; set; }
    
    public int Count { get; set; }
}

internal class ConsoleJobHandler : IJobHandler<ConsoleJob>
{
    public async Task HandleAsync(ConsoleJob data, CancellationToken stoppingToken)
    {
        for (int i = 0; i < data.Count; i++)
        {
            Console.WriteLine($"{i + 1}: " +data.Text);
            await Task.Delay(10000, stoppingToken);
        }
    }
}