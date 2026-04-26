using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;

namespace Kododo.RunWay.Dashboard.API.Recurrences.GetRecurrences;

internal sealed class GetRecurrencesHandler(IRecurrenceStore recurrenceStore)
    : IRequestHandler<GetRecurrences, PagedResult<RecurrenceDto>>
{
    public async Task<PagedResult<RecurrenceDto>> HandleAsync(GetRecurrences request, CancellationToken stoppingToken)
    {
        var pagination = new Pagination(request.Page);
        var result = await recurrenceStore.GetRecurrencesAsync(pagination, stoppingToken);
        
        var totalPages = (int)Math.Ceiling((double)result.TotalItems / pagination.PageSize);
        var items = result.Items.Select(RecurrenceDto.FromRecurrence).ToArray();
        return new PagedResult<RecurrenceDto>(items, result.TotalItems, totalPages, pagination.PageSize);
    }
}