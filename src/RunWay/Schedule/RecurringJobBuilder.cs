using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;
namespace Kododo.RunWay.Schedule;

public interface IRecurringJobBuilder<T> : IJobBuilder<T>
{
    Task SetRecurrenceAsync(string key, string cronExpression, CancellationToken stoppingToken);
}

internal sealed class RecurringJobBuilder<T> : JobBuilder<T>, IRecurringJobBuilder<T>
{
    private readonly T _data;
    private readonly IStore _store;
    private readonly IRecurrenceCalculator _recurrenceCalculator;
    private readonly RunWayOptions _options;

    public RecurringJobBuilder(T data, IStore store, IRecurrenceCalculator recurrenceCalculator, RunWayOptions options) : base(options)
    {
        _data = data;
        _store = store;
        _recurrenceCalculator = recurrenceCalculator;
        _options = options;
    }

    public async Task SetRecurrenceAsync(string key, string cronExpression, CancellationToken stoppingToken)
    {
        var recurrenceId = new RecurrenceId(key);
        var jobData = _options.JobDataSerializer.Serialize(_data);
        var jobOptions = JobOptionsBuilder.Build();
        var recurrence = Recurrence.CreateNew(recurrenceId, cronExpression, jobData, jobOptions);
        var existingRecurrence = await _store.FindRecurrenceAsync(recurrenceId, stoppingToken);

        if (existingRecurrence == null)
        {
            if (Transactional)
            {
                await using var transaction = await _store.BeginTransactionAsync(stoppingToken);
                await CreateAsync(recurrence, stoppingToken);
                await transaction.CommitAsync(stoppingToken);
            }
            else
            {
                await CreateAsync(recurrence, stoppingToken);
            }
        }
        else
        {
            var changed = !existingRecurrence.Equals(recurrence);

            if (changed)
            {
                if (Transactional)
                {
                    await using var transaction = await _store.BeginTransactionAsync(stoppingToken);
                    await UpdateAsync(recurrence, existingRecurrence, stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                }
                else
                {
                    await UpdateAsync(recurrence, existingRecurrence, stoppingToken);
                }
            }
        }
    }

    private async Task UpdateAsync(Recurrence recurrence, Recurrence existingRecurrence, CancellationToken stoppingToken)
    {
        var cronChanged = !existingRecurrence.Rule.Equals(recurrence.Rule);
        existingRecurrence.Update(recurrence.Rule, recurrence.Data, recurrence.Options);
        var currentJob = existingRecurrence.CurrentJob;

        if (currentJob is { IsWaiting: true })
        {
            // The waiting job takes the new data and options; with a new expression it also moves to the new time.
            // No second job is created, so nothing runs twice or with the old settings.
            var scheduledAt = cronChanged
                ? _recurrenceCalculator.CalculateNextOccurrence(recurrence.Rule, DateTimeOffset.UtcNow)
                : currentJob.ScheduledAt;
            currentJob.Reconfigure(recurrence.Data, recurrence.Options, scheduledAt);
            try
            {
                await _store.UpdateWithoutTransactionAsync(currentJob, stoppingToken);
            }
            catch (Exception ex) when (_store.IsConcurrencyException(ex))
            {
                // A runner started the job in the meantime. It runs with the old settings; the next one is created
                // from the updated recurrence when it completes.
            }
        }
        else if (currentJob is null || currentJob.Status.IsCompleted())
        {
            var nextOccurrence = _recurrenceCalculator.CalculateNextOccurrence(recurrence.Rule, DateTimeOffset.UtcNow);
            var job = await _store.CreateWithoutTransactionAsync(Job.CreateNew(existingRecurrence, nextOccurrence), stoppingToken);
            existingRecurrence.AssignJob(job);
        }
        // A running job keeps its settings; the next one is created from the updated recurrence when it completes.

        await _store.UpdateRecurrenceWithoutTransactionAsync(existingRecurrence, stoppingToken);
    }

    private async Task CreateAsync(Recurrence recurrence, CancellationToken stoppingToken)
    {
        await _store.CreateRecurrenceWithoutTransactionAsync(recurrence, stoppingToken);
        var nextOccurrence = _recurrenceCalculator.CalculateNextOccurrence(recurrence.Rule, DateTimeOffset.UtcNow);
        var job = Job.CreateNew(recurrence, nextOccurrence);
        job = await _store.CreateWithoutTransactionAsync(job, stoppingToken);
        recurrence.AssignJob(job);
        await _store.UpdateRecurrenceWithoutTransactionAsync(recurrence, stoppingToken);
    }
}