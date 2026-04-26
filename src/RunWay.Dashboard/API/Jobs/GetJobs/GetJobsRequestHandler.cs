using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Kododo.RunWay.Dashboard.Utils;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetJobs;

internal sealed class GetJobsRequestHandler(IJobStore store) : IRequestHandler<GetJobs, PagedResult<JobListItem>>
{
    public async Task<PagedResult<JobListItem>> HandleAsync(GetJobs request, CancellationToken cancellationToken)
    {
        var query = PrepareQuery(request.Category);
        var jobsQuery = query.ToMany(request.Page);
        
        var result = await store.GetManyAsync(jobsQuery, cancellationToken);
        
        var totalPages = (int)Math.Ceiling((double)result.TotalItems / jobsQuery.Pagination.PageSize);
        var items = result.Items.Select(JobListItem.FromJob).ToArray();
        return new PagedResult<JobListItem>(items, result.TotalItems, totalPages, jobsQuery.Pagination.PageSize);
    }

    private static Query PrepareQuery(JobCategory category)
    {
        var query = new Query();
        switch (category)
        {
            case JobCategory.Pending:
                return query
                    .WhereStatus(JobStatus.Scheduled)
                    .OrderByDescending(JobSortingField.Priority)
                    .OrderBy(JobSortingField.ScheduledAt);
            
            case JobCategory.Running:
                return query
                    .WhereStatus(JobStatus.Running)
                    .OrderByDescending(JobSortingField.ModifiedAt);
            
            case JobCategory.Retrying:
                return query
                    .WhereStatus(JobStatus.Scheduled, JobStatus.Running)
                    .WhereMinRetriesCount(1)
                    .OrderByDescending(JobSortingField.Status)
                    .OrderByDescending(JobSortingField.Priority)
                    .OrderBy(JobSortingField.ScheduledAt);
            
            case JobCategory.Succeeded:
                return query
                    .WhereStatus(JobStatus.Succeeded)
                    .OrderByDescending(JobSortingField.ModifiedAt);
            
            case JobCategory.Failed:
                return query
                    .WhereStatus(JobStatus.Failed)
                    .OrderByDescending(JobSortingField.ModifiedAt);
            
            default:
                throw new ArgumentOutOfRangeException(nameof(category), category, null);
        }
    }
}