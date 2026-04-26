namespace Kododo.RunWay.Core.Recurrences;

public interface IRecurrenceCalculator
{
    DateTimeOffset CalculateNextOccurrence(string expression, DateTimeOffset from);
}