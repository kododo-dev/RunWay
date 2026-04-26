using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetStatusesCounts;

internal class GetStatusesCountsHandler(IJobStore store) : IRequestHandler<GetStatusesCounts, StatusesCountsResult>
{
    public async Task<StatusesCountsResult> HandleAsync(GetStatusesCounts request, CancellationToken cancellationToken)
    {
        var retrying = await store.GetCountAsync(new JobsFilter
        {
            MinRetriesCount = 1,
            Statuses =
            [
                JobStatus.Scheduled,
                JobStatus.Running
            ]
        }, cancellationToken);
        
        var pending = await store.GetCountAsync(new JobsFilter
        {
            Statuses = [JobStatus.Scheduled]
        }, cancellationToken);

        var running = await store.GetCountAsync(new JobsFilter
        {
            Statuses = [JobStatus.Running]
        }, cancellationToken);
        
        var succeeded = await store.GetCountAsync(new JobsFilter
        {
            Statuses = [JobStatus.Succeeded]
        }, cancellationToken);
        
        var failed = await store.GetCountAsync(new JobsFilter
        {
            Statuses = [JobStatus.Failed]
        }, cancellationToken);
        
        return new StatusesCountsResult(pending, running, succeeded, failed, retrying);
    }
}