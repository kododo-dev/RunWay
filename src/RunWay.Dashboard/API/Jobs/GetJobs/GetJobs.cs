using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetJobs;

internal enum JobCategory
{
    Pending,
    Running,
    Retrying,
    Succeeded,
    Failed
}

internal sealed record GetJobs(JobCategory Category, int Page) : IRequest<PagedResult<JobListItem>>;