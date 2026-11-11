using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Store;

namespace Kododo.RunWay.Schedule;

public interface IScheduleJobBuilder<T> : IJobBuilder<T>
{
    // The settings return the schedule builder, so a chain can end with ScheduleAsync.
    new IScheduleJobBuilder<T> AsTransactional(bool transactional);
    new IScheduleJobBuilder<T> WithPriority(int priority);
    new IScheduleJobBuilder<T> WithRetryDelaysInSeconds(params int[] retryDelaysInSeconds);
    new IScheduleJobBuilder<T> WithTimeout(TimeSpan timeout);

    Task<JobId> ScheduleAsync(DateTimeOffset scheduledAt, CancellationToken stoppingToken);
    Task<JobId> ScheduleAsync(CancellationToken stoppingToken);
}

internal sealed class ScheduleJobBuilder<T> : JobBuilder<T>, IScheduleJobBuilder<T>
{
    private readonly T _data;
    private readonly IStore _store;
    private readonly RunWayOptions _options;

    public ScheduleJobBuilder(T data, IStore store, RunWayOptions options) : base(options)
    {
        _data = data;
        _store = store;
        _options = options;
    }

    public new IScheduleJobBuilder<T> AsTransactional(bool transactional) { base.AsTransactional(transactional); return this; }

    public new IScheduleJobBuilder<T> WithPriority(int priority) { base.WithPriority(priority); return this; }

    public new IScheduleJobBuilder<T> WithRetryDelaysInSeconds(params int[] retryDelaysInSeconds)
    {
        base.WithRetryDelaysInSeconds(retryDelaysInSeconds);
        return this;
    }

    public new IScheduleJobBuilder<T> WithTimeout(TimeSpan timeout) { base.WithTimeout(timeout); return this; }

    public async Task<JobId> ScheduleAsync(DateTimeOffset scheduledAt, CancellationToken stoppingToken)
    {
        var jobData = _options.JobDataSerializer.Serialize(_data);
        var jobOptions = JobOptionsBuilder.Build();
        var job = Job.CreateNew(jobData, jobOptions, scheduledAt);
        job = Transactional
            ? await _store.CreateAsync(job, stoppingToken)
            : await _store.CreateWithoutTransactionAsync(job, stoppingToken);

        return job.Id;
    }
    
    public async Task<JobId> ScheduleAsync(CancellationToken stoppingToken)
    {
        return await ScheduleAsync(DateTimeOffset.UtcNow, stoppingToken);
    }
}