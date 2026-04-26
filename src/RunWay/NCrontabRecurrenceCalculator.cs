using Kododo.RunWay.Core.Recurrences;
using NCrontab;

namespace Kododo.RunWay;

internal sealed class NCrontabRecurrenceCalculator : IRecurrenceCalculator
{
    public DateTimeOffset CalculateNextOccurrence(string expression, DateTimeOffset from)
    {
        var schedule = CrontabSchedule.Parse(expression);
        var nextOccurrence = schedule.GetNextOccurrence(from.UtcDateTime);
        return new DateTimeOffset(nextOccurrence, TimeSpan.Zero);
    }
}