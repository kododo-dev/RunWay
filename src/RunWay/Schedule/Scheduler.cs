using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;

namespace Kododo.RunWay.Schedule;

public interface IScheduler
{
    IScheduleJobBuilder<T> Job<T>(T data);
    IRecurringJobBuilder<T> Recurrence<T>(T data);
}

internal sealed class Scheduler(IStore store, IRecurrenceCalculator recurrenceCalculator, RunWayOptions options) : IScheduler
{
    public IScheduleJobBuilder<T> Job<T>(T data)
    {
        return new ScheduleJobBuilder<T>(data, store, options);
    }

    public IRecurringJobBuilder<T> Recurrence<T>(T data)
    {
        return new RecurringJobBuilder<T>(data, store, recurrenceCalculator, options);
    }
}