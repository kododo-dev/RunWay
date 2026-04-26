using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;

namespace Kododo.RunWay.Dashboard.API.Recurrences.GetRecurrenceJobs;

internal sealed class GetRecurrenceJobsHandler(IJobStore store) : IRequestHandler<GetRecurrenceJobs, PagedResult<JobListItem>>
{
    public async Task<PagedResult<JobListItem>> HandleAsync(GetRecurrenceJobs request, CancellationToken cancellationToken)
    {
        var query = new JobsQuery
        {
            Pagination = new Pagination(request.Page),
            Filter = new JobsFilter
            {
                RecurrenceId = new RecurrenceId(request.Id)
            },
            Sorting = [new JobsSorting(JobSortingField.ScheduledAt, false)]
        };
        
        var result = await store.GetManyAsync(query, cancellationToken);
        
        var totalPages = (int)Math.Ceiling((double)result.TotalItems / query.Pagination.PageSize);
        var items = result.Items.Select(JobListItem.FromJob).ToArray();
        return new PagedResult<JobListItem>(items, result.TotalItems, totalPages, query.Pagination.PageSize);
    }
}