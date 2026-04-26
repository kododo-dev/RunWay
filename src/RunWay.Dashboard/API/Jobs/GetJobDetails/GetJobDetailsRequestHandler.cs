using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Store;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetJobDetails;

internal sealed class GetJobDetailsRequestHandler(IJobStore store) : IRequestHandler<GetJobDetails, Details>
{
    public async Task<Details> HandleAsync(GetJobDetails request, CancellationToken cancellationToken)
    {
        var id = new JobId(request.Id);
        var jobDetails = await store.GetDetailsAsync(id, cancellationToken);
        
        return new Details(
            jobDetails.Id.Value,
            JobType.FromFullTypeName(jobDetails.Data.Type),
            jobDetails.Data.Data,
            jobDetails.Status,
            jobDetails.Options.Priority,
            jobDetails.RetriesCount,
            jobDetails.ScheduledAt.UtcDateTime,
            jobDetails.ModifiedAt.UtcDateTime,
            jobDetails.Audit.Select(
                a => new AuditItem(a.Time.UtcDateTime, a.Type, a.Details))
                .ToArray()
            );
    }
}