namespace Kododo.RunWay.Core.Store;

public interface IStore : IJobStore, INoTransactionalJobStore, IRunnerStore, IRecurrenceStore
{
    Task InitializeAsync(CancellationToken stoppingToken);
    bool IsConcurrencyException(Exception ex);
    Task<ITransaction> BeginTransactionAsync(CancellationToken stoppingToken);
}