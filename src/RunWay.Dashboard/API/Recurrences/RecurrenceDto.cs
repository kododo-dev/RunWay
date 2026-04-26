using Kododo.RunWay.Core.Recurrences;

namespace Kododo.RunWay.Dashboard.API.Recurrences;

internal sealed record RecurrenceDto(
    string Id,
    string Rule,
    JobType Type,
    string Data)
{
    public static RecurrenceDto FromRecurrence(Recurrence x)
    {
        return new RecurrenceDto(
            x.Id.Value,
            x.Rule,
            JobType.FromFullTypeName(x.Data.Type),
            x.Data.Data);
    }
}