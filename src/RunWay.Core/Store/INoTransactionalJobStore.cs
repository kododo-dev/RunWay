using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Core.Store;

public interface INoTransactionalJobStore
{
    Task<Job> CreateWithoutTransactionAsync(Job job, CancellationToken stoppingToken);
    
    Task<Job> UpdateWithoutTransactionAsync(Job job, CancellationToken stoppingToken);
}