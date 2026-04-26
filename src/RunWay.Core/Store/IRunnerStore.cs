using Kododo.RunWay.Core.Runners;

namespace Kododo.RunWay.Core.Store;

public interface IRunnerStore
{
    Task<RunnerId> RegisterRunnerAsync(string name, IReadOnlyCollection<string> types, DateTimeOffset expiresAt, CancellationToken stoppingToken);
    Task KeepAliveAsync(RunnerId runnerId, DateTimeOffset expiresAt, CancellationToken stoppingToken);
    Task<IReadOnlyCollection<Runner>> GetAllRunnersAsync(CancellationToken stoppingToken);
    Task RemoveRunnerAsync(RunnerId runnerId, CancellationToken stoppingToken);
    Task<RunnerDetails> GetRunnerDetailsAsync(RunnerId runnerId, CancellationToken stoppingToken);
}