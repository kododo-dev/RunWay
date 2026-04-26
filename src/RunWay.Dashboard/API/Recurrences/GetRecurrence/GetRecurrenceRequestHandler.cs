using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;

namespace Kododo.RunWay.Dashboard.API.Recurrences.GetRecurrence;

internal sealed class GetRecurrenceRequestHandler(IRecurrenceStore store) : IRequestHandler<GetRecurrence, RecurrenceDto>
{
    public async Task<RecurrenceDto> HandleAsync(GetRecurrence request, CancellationToken cancellationToken)
    {
        var recurrence = await store.FindRecurrenceAsync(new RecurrenceId(request.Id), cancellationToken);
        return RecurrenceDto.FromRecurrence(recurrence!);
    }
}