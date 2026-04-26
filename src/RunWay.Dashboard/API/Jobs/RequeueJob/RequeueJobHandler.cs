using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Store;

namespace Kododo.RunWay.Dashboard.API.Jobs.RequeueJob;

internal sealed class RequeueJobHandler(IJobStore store) : IRequestHandler<RequeueJob>
{
    public async Task HandleAsync(RequeueJob request, CancellationToken cancellationToken)
    {
        var id = new JobId(request.Id);
        var job = await store.FindAsync(id, cancellationToken)
                  ?? throw new InvalidOperationException($"Job {request.Id} not found.");

        job.Requeue();
        await store.UpdateAsync(job, cancellationToken);
    }
}
