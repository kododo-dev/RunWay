using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kododo.RunWay.Runner;

internal abstract class BaseTask(IServiceProvider serviceProvider, ILogger logger)
{
    public async Task Run(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            
            try
            {
                await ExecuteAsync(scope, store, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                if (!store.IsConcurrencyException(ex))
                {
                    logger.LogError(ex, "An error occurred while executing the job runner.");
                }
            }
        }
    }

    protected static async Task OnCompletedAsync(Job job, AsyncServiceScope scope, IStore store, CancellationToken stoppingToken)
    {
        if (job.RecurrenceId != null && job.Status.IsCompleted())
        {
            await TriggerRecurrenceAsync(job, job.RecurrenceId.Value, scope, store, stoppingToken);
        }
    }
    
    private static async Task TriggerRecurrenceAsync(Job job, RecurrenceId recurrenceId, AsyncServiceScope scope, IStore store, CancellationToken stoppingToken)
    {
        var recurrence = await store.FindRecurrenceAsync(recurrenceId, stoppingToken);
        if (recurrence != null && recurrence.CurrentJob?.Id == job.Id)
        {
            await TriggerRecurrenceAsync(recurrence, scope, store, stoppingToken);
        }
    }
    
    private static async Task TriggerRecurrenceAsync(Recurrence recurrence, AsyncServiceScope scope, IStore store,
        CancellationToken stoppingToken)
    {
        var recurrenceCalculator = scope.ServiceProvider.GetRequiredService<IRecurrenceCalculator>();
        var nextOccurrence = recurrenceCalculator.CalculateNextOccurrence(recurrence.Rule, DateTimeOffset.UtcNow);
            
        await using var transaction = await store.BeginTransactionAsync(stoppingToken);

        var job = Job.CreateNew(recurrence, nextOccurrence);
        job = await store.CreateWithoutTransactionAsync(job, stoppingToken);
        recurrence.AssignJob(job);
        await store.UpdateRecurrenceWithoutTransactionAsync(recurrence, stoppingToken);
            
        await transaction.CommitAsync(stoppingToken);
    }

    protected abstract Task ExecuteAsync(AsyncServiceScope scope, IStore store, CancellationToken stoppingToken);
}