using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Jobs.Audit;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Kododo.RunWay.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.EntityFramework;

internal sealed class Store(JobsDbContext db) : IStore
{
    public async Task<Job?> FindAsync(JobId jobId, CancellationToken stoppingToken)
    {
        var id = ParseId(jobId);
        var dbJob = await db.Jobs
            .Include(x => x.Recurrence)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, stoppingToken);
        
        return dbJob == null ? null : ToJob(dbJob);
    }
    
    public async Task<RunnerId> RegisterRunnerAsync(string name,  IReadOnlyCollection<string> types, DateTimeOffset expiresAt, CancellationToken stoppingToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        var dbRunner = new DbRunner
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            ExpiresAt = expiresAt.UtcDateTime,
            JobTypes = types.Select(x => new DbRunnerJobType
            {
                JobType = x
            }).ToList()
        };
        
        db.Runners.Add(dbRunner);
        await db.SaveChangesAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);
        return new RunnerId(dbRunner.Id.ToString());
    }

    public async Task KeepAliveAsync(RunnerId runnerId, DateTimeOffset expiresAt, CancellationToken stoppingToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        await db.Runners
            .Where(x => x.Id == ParseId(runnerId))
            .ExecuteUpdateAsync(
            x => 
                x.SetProperty(w => w.LastSeenAt, DateTime.UtcNow)
                    .SetProperty(w => w.ExpiresAt, expiresAt.UtcDateTime),
            stoppingToken);
        await transaction.CommitAsync(stoppingToken);
    }

    public async Task RemoveRunnerAsync(RunnerId runnerId, CancellationToken stoppingToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        await db.Runners
            .Where(x => x.Id == ParseId(runnerId))
            .ExecuteDeleteAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);
    }

    public async Task<RunnerDetails> GetRunnerDetailsAsync(RunnerId runnerId, CancellationToken stoppingToken)
    {
        var id = ParseId(runnerId);
        return await db.Runners
            .AsNoTracking()
            .Include(x => x.JobTypes)
            .Where(x => x.Id == id)
            .Select(runner => new RunnerDetails(
                runnerId,
                runner.Name,
                runner.CreatedAt,
                runner.LastSeenAt,
                runner.JobTypes.Select(jt => jt.JobType).ToArray(),
                db.Jobs
                    .AsNoTracking()
                    .Include(x => x.Recurrence)
                    .Where(j => j.RunnerId == runner.Id)
                    .Select(job => ToJob(job))
                    .ToArray(),
                runner.ExpiresAt
            ))
            .FirstAsync(stoppingToken);
    }

    public async Task<IReadOnlyCollection<Runner>> GetAllRunnersAsync(CancellationToken stoppingToken)
    {
        var runners = await db.Runners
            .AsNoTracking()
            .Include(x => x.JobTypes)
            .ToArrayAsync(stoppingToken);

        return runners.Select(w => new Runner(
            ToRunnerId(w.Id)!.Value,
            w.Name,
            w.LastSeenAt,
            w.ExpiresAt
        )).ToArray();
    }

    public async Task InitializeAsync(CancellationToken stoppingToken)
    {
        await db.Database.MigrateAsync(stoppingToken);
    }
    
    public async Task<Job> CreateWithoutTransactionAsync(Job job, CancellationToken stoppingToken)
    {
        long? recurrenceId = null;

        if (job.RecurrenceId.HasValue)
        {
            recurrenceId = await db.Recurrences
                .Where(x => x.Key == job.RecurrenceId.Value.Value)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(stoppingToken);
        }
        
        var dbJob = new DbJob
        {
            Version = job.Version,
            ModifiedAt = DateTime.UtcNow,
            Status = JobStatus.Scheduled,
            Type = job.Data.Type,
            Data = job.Data.Data,
            ScheduledAt = job.ScheduledAt.UtcDateTime,
            Priority = job.Options.Priority,
            Options = new DbJobOptions(job.Options.RetryDelaysInSeconds.ToArray(), job.Options.Timeout),
            ReccurenceId = recurrenceId
        };

        db.Jobs.Add(dbJob);
        await db.SaveChangesAsync(stoppingToken);

        foreach (var auditRecord in job.PendingEvents)
        {
            db.JobsAudit.Add(ToDbJobAudit(auditRecord, dbJob.Id));
        }

        await db.SaveChangesAsync(stoppingToken);

        return ToJob(dbJob);
    }

    public async Task<Job> UpdateWithoutTransactionAsync(Job job, CancellationToken stoppingToken)
    {
        var id = ParseId(job.Id);
        var dbJob = await db.Jobs
            .Include(x => x.Recurrence)
            .FirstAsync(x => x.Id == id, stoppingToken);
        
        if(dbJob.Version != job.Version)
        {
            throw new DbUpdateConcurrencyException("The job has a newer version");
        }

        dbJob.Version = job.Version + 1;
        dbJob.ModifiedAt = DateTime.UtcNow;
        dbJob.Status = job.Status;
        dbJob.ScheduledAt = job.ScheduledAt.UtcDateTime;
        dbJob.RetriesCount = job.RetriesCount;
        dbJob.RunnerId = job.RunnerId != null ? ParseId(job.RunnerId.Value) : null;

        foreach (var auditRecord in job.PendingEvents)
        {
            db.JobsAudit.Add(ToDbJobAudit(auditRecord, id));
        }

        await db.SaveChangesAsync(stoppingToken);
        var result = await FindAsync(job.Id, stoppingToken) ?? throw new InvalidOperationException("Job not found after update");
        return result;
    }

    public async Task<Job> CreateAsync(Job job, CancellationToken stoppingToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        var result = await this.CreateWithoutTransactionAsync(job, stoppingToken);
        await transaction.CommitAsync(stoppingToken);
        return result;
    }

    public async Task<Job> UpdateAsync(Job job, CancellationToken stoppingToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        var result = await this.UpdateWithoutTransactionAsync(job, stoppingToken);
        await transaction.CommitAsync(stoppingToken);
        return result;
    }

    public Task<Job?> GetOneOrDefaultAsync(JobQuery query, CancellationToken stoppingToken)
    {
        var result = db.Jobs
            .Include(x => x.Recurrence)
            .AsNoTracking();

        if (query.Filter != null)
        {
            result = ApplyFilter(result, query.Filter);
        }
        
        if(query.Sorting != null && query.Sorting.Count > 0)
        {
            result = ApplySorting(result, query.Sorting);
        }
        
        return result.Select(x => ToJob(x)).FirstOrDefaultAsync(stoppingToken);
    }

    public async Task<PagedResult<Job>> GetManyAsync(JobsQuery query, CancellationToken stoppingToken)
    {
        var result = db.Jobs
            .Include(x => x.Recurrence)
            .AsNoTracking();

        if (query.Filter != null)
        {
            result = ApplyFilter(result, query.Filter);
        }
        
        var count = await result.CountAsync(stoppingToken);
        
        if(query.Sorting != null && query.Sorting.Count > 0)
        {
            result = ApplySorting(result, query.Sorting);
        }
        
        result = result
            .Skip((query.Pagination.Page - 1) * query.Pagination.PageSize)
            .Take(query.Pagination.PageSize);

        var pageItems = await result.Select(x => ToJob(x)).ToArrayAsync(stoppingToken);
        return new PagedResult<Job>(pageItems, count);
    }

    public Task<int> GetCountAsync(JobsFilter filter, CancellationToken stoppingToken)
    {
        var query = db.Jobs
            .AsNoTracking()
            .Include(x => x.Recurrence)
            .AsQueryable();
        
        query = ApplyFilter(query, filter);
        return query.CountAsync(stoppingToken);
    }

    public async Task<JobDetails> GetDetailsAsync(JobId jobId, CancellationToken stoppingToken)
    {
        var id = ParseId(jobId);
        var result = await db.Jobs
            .Include(x => x.Recurrence)
            .AsNoTracking()
            .Include(x => x.Runner)
            .Where(x => x.Id == id)
            .Select(job => new
            {
                Job = job,
                Audits = db.JobsAudit
                    .Where(a => a.JobId == job.Id)
                    .OrderByDescending(a => a.Time)
                    .ToArray()
            })
            .FirstAsync(stoppingToken);

        return new JobDetails(
            jobId,
            new JobData(result.Job.Type, result.Job.Data),
            new JobOptions(result.Job.Priority, result.Job.Options.RetryDelays, result.Job.Options.Timeout),
            result.Job.Recurrence == null ? null : ToRecurrenceId(result.Job.Recurrence.Key),
            result.Job.Status,
            result.Job.Runner != null ? ToRunnerInfo(result.Job.Runner) : null,
            result.Job.RetriesCount,
            result.Job.ModifiedAt,
            result.Job.ScheduledAt,
            result.Audits
                .Select(a => new JobAuditRecord(a.Time, a.Type, a.Details))
                .ToArray()
        );
    }

    public async Task DeleteAsync(JobsFilter filter, CancellationToken stoppingToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        var jobsToDelete = ApplyFilter(db.Jobs, filter);
        await jobsToDelete.ExecuteDeleteAsync(stoppingToken);
        await transaction.CommitAsync(stoppingToken);
    }

    public async Task<Recurrence?> FindRecurrenceAsync(RecurrenceId id, CancellationToken stoppingToken)
    {
        var dbRecurrence = await db.Recurrences
            .Include(x => x.Job)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == id.Value, stoppingToken);
        
        return dbRecurrence == null ? null : ToRecurrence(dbRecurrence);
    }

    public async Task<Recurrence> CreateRecurrenceWithoutTransactionAsync(Recurrence recurrence, CancellationToken stoppingToken)
    {
        var dbRecurrence = new DbRecurrence
        {
            Key = recurrence.Id.Value,
            Version = recurrence.Version,
            Expression = recurrence.Rule,
            Type = recurrence.Data.Type,
            Data = recurrence.Data.Data,
            Options = recurrence.Options,
            JobId = recurrence.CurrentJob?.Id != null ? ParseId(recurrence.CurrentJob.Id) : null
        };

        db.Recurrences.Add(dbRecurrence);
        await db.SaveChangesAsync(stoppingToken);
        var result = await FindRecurrenceAsync(recurrence.Id, stoppingToken) ?? throw new InvalidOperationException("Recurrence not found after create");
        return result;
    }

    public async Task<Recurrence> UpdateRecurrenceWithoutTransactionAsync(Recurrence recurrence, CancellationToken stoppingToken)
    {
        var dbRecurrence = await db.Recurrences
            .FirstAsync(x => x.Key == recurrence.Id.Value, stoppingToken);
        
        if(dbRecurrence.Version != recurrence.Version)
        {
            throw new DbUpdateConcurrencyException("The recurrence has a newer version");
        }

        dbRecurrence.Version = recurrence.Version + 1;
        dbRecurrence.ModifiedAt = DateTime.UtcNow;
        dbRecurrence.JobId = recurrence.CurrentJob?.Id != null ? ParseId(recurrence.CurrentJob.Id) : null;
        dbRecurrence.Data = recurrence.Data.Data;
        dbRecurrence.Options = recurrence.Options;
        dbRecurrence.Type = recurrence.Data.Type;
        dbRecurrence.Expression = recurrence.Rule;

        await db.SaveChangesAsync(stoppingToken);
        var result = await FindRecurrenceAsync(recurrence.Id, stoppingToken) ?? throw new InvalidOperationException("Recurrence not found after update");
        return result;
    }

    public async Task<PagedResult<Recurrence>> GetRecurrencesAsync(Pagination pagination, CancellationToken stoppingToken)
    {
        var query = db.Recurrences
            .Include(x => x.Job)
            .AsNoTracking()
            .Select(r => ToRecurrence(r));

        var count = await query.CountAsync(stoppingToken);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToArrayAsync(stoppingToken);

        return new PagedResult<Recurrence>(items, count);
    }

    public bool IsConcurrencyException(Exception ex)
    {
        return ex is DbUpdateConcurrencyException;
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken stoppingToken)
    {
        var dbTransaction = await db.Database.BeginTransactionAsync(stoppingToken);
        return new Transaction(dbTransaction);
    }

    private static long ParseId(JobId jobId) => long.Parse(jobId.Value);
    
    private static long ParseId(RunnerId runnerId) => long.Parse(runnerId.Value);

    private static DbJobAudit ToDbJobAudit(JobAuditRecord auditRecord, long jobId)
    {
        return new DbJobAudit
        {
            JobId = jobId,
            Time = auditRecord.Time.UtcDateTime,
            Type = auditRecord.Type,
            Details = auditRecord.Details
        };
    }

    private IQueryable<DbJob> ApplyFilter(IQueryable<DbJob> query, JobsFilter filter)
    {
        if (filter.MinRetriesCount.HasValue)
        {
            query = query.Where(x => x.RetriesCount >= filter.MinRetriesCount.Value);
        }
        
        if (filter.Statuses != null && filter.Statuses.Count > 0)
        {
            query = query.Where(x => filter.Statuses.Contains(x.Status));
        }

        if (filter.MaxScheduledAt.HasValue)
        {
            query = query.Where(x => x.ScheduledAt <= filter.MaxScheduledAt.Value.UtcDateTime);
        }
        
        if (filter.MaxModifiedAt.HasValue)
        {
            query = query.Where(x => x.ModifiedAt <= filter.MaxModifiedAt.Value);
        }

        if (filter.SuitableForRunnerId.HasValue)
        {
            var jobTypes = db.RunnerJobTypes
                .Where(wt => wt.RunnerId == ParseId(filter.SuitableForRunnerId.Value))
                .Select(wt => wt.JobType);
            
            query = query.Where(x => jobTypes.Contains(x.Type));
        }
        
        if (filter.AssignedRunnerId.HasValue)
        {
            query = query.Where(x => x.RunnerId == ParseId(filter.AssignedRunnerId.Value));
        }
        
        if (filter.RecurrenceId.HasValue)
        {
            query = query.Where(x => x.Recurrence != null && x.Recurrence.Key == filter.RecurrenceId.Value.Value);
        }

        return query;
    }
    
        private static IQueryable<DbJob> ApplySorting(IQueryable<DbJob> query, IReadOnlyCollection<JobsSorting> sorting)
    {
        IOrderedQueryable<DbJob>? orderedResult = null;
        foreach (var s in sorting)
        {
            if (s.Ascending)
            {
                orderedResult = s.Field switch
                {
                    JobSortingField.ModifiedAt => orderedResult == null ? query.OrderBy(x => x.ModifiedAt) : orderedResult.ThenBy(x => x.ModifiedAt),
                    JobSortingField.ScheduledAt => orderedResult == null ? query.OrderBy(x => x.ScheduledAt) : orderedResult.ThenBy(x => x.ScheduledAt),
                    JobSortingField.Priority => orderedResult == null ? query.OrderBy(x => x.Priority) : orderedResult.ThenBy(x => x.Priority),
                    JobSortingField.Status => orderedResult == null ? query.OrderBy(x => x.Status) : orderedResult.ThenBy(x => x.Status),
                    _ => throw new ArgumentOutOfRangeException(nameof(s.Field), $"Unsupported sorting field: {s.Field}")
                };
            }
            else
            {
                orderedResult = s.Field switch
                {
                    JobSortingField.ModifiedAt => orderedResult == null ? query.OrderByDescending(x => x.ModifiedAt) : orderedResult.ThenByDescending(x => x.ModifiedAt),
                    JobSortingField.ScheduledAt => orderedResult == null ? query.OrderByDescending(x => x.ScheduledAt) : orderedResult.ThenByDescending(x => x.ScheduledAt),
                    JobSortingField.Priority => orderedResult == null ? query.OrderByDescending(x => x.Priority) : orderedResult.ThenByDescending(x => x.Priority),
                    JobSortingField.Status => orderedResult == null ? query.OrderByDescending(x => x.Status) : orderedResult.ThenByDescending(x => x.Status),
                    _ => throw new ArgumentOutOfRangeException(nameof(s.Field), $"Unsupported sorting field: {s.Field}")
                };
            }
        }

        return orderedResult ?? query;
    }

    private static JobId ToJobId(long id)
    {
        var jobId = new JobId(id.ToString());
        return jobId;
    }
    
    private static RecurrenceId ToRecurrenceId(string id)
    {
        var recurrenceId = new RecurrenceId(id);
        return recurrenceId;
    }
    
    private static RunnerId? ToRunnerId(long? id)
    {
        if (id == null)
        {
            return null;
        }
        
        var runnerId = new RunnerId(id.Value.ToString());
        return runnerId;
    }

    private static Recurrence ToRecurrence(DbRecurrence dbRecurrence)
    {
        return new Recurrence(
            ToRecurrenceId(dbRecurrence.Key),
            dbRecurrence.Version,
            dbRecurrence.Expression,
            new JobData(dbRecurrence.Type, dbRecurrence.Data),
            dbRecurrence.Options,
            dbRecurrence.Job == null ? null : ToJob(dbRecurrence.Job)
        );
    }
    
    private static Job ToJob(DbJob dbJob)
    {
        var jobId = ToJobId(dbJob.Id);
        var data = new JobData(dbJob.Type, dbJob.Data);
        var metadata = new JobOptions(dbJob.Priority, dbJob.Options.RetryDelays, dbJob.Options.Timeout);
        var recurrenceId = dbJob.Recurrence != null ? ToRecurrenceId(dbJob.Recurrence.Key) : (RecurrenceId?)null;
        return new Job(jobId, dbJob.Version, data, metadata, recurrenceId, dbJob.Status, ToRunnerId(dbJob.RunnerId), dbJob.ModifiedAt, dbJob.ScheduledAt, dbJob.RetriesCount);
    }

    private static RunnerInfo ToRunnerInfo(DbRunner dbRunner)
    {
        return new RunnerInfo(ToRunnerId(dbRunner.Id)!.Value, dbRunner.Name);
    }
}