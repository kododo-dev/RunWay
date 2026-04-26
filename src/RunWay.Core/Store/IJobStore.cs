using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Store.Queries;

namespace Kododo.RunWay.Core.Store;

public interface IJobStore
{
    Task<Job> CreateAsync(Job job, CancellationToken stoppingToken);
    
    Task<Job> UpdateAsync(Job job, CancellationToken stoppingToken);
    
    Task<Job?> GetOneOrDefaultAsync(JobQuery query, CancellationToken stoppingToken);
    
    Task<PagedResult<Job>> GetManyAsync(JobsQuery query, CancellationToken stoppingToken);

    Task<int> GetCountAsync(JobsFilter filter, CancellationToken stoppingToken);
    
    Task<Job?> FindAsync(JobId jobId, CancellationToken stoppingToken);

    Task<JobDetails> GetDetailsAsync(JobId jobId, CancellationToken stoppingToken);
    Task DeleteAsync(JobsFilter filter, CancellationToken stoppingToken);
}