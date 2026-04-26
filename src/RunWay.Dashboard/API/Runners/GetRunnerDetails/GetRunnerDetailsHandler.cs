using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;

namespace Kododo.RunWay.Dashboard.API.Runners.GetRunnerDetails;

internal sealed class GetRunnerDetailsHandler(IRunnerStore store) : IRequestHandler<GetRunnerDetails, RunnerDetailsDto>
{
    public async Task<RunnerDetailsDto> HandleAsync(GetRunnerDetails request, CancellationToken cancellationToken)
    {
        var id = new RunnerId(request.Id);
        var details = await store.GetRunnerDetailsAsync(id, cancellationToken);
        return new RunnerDetailsDto(
            details.Id.Value,
            details.Name,
            details.Status,
            details.CreatedAt.UtcDateTime,
            details.LastSeenAt.UtcDateTime,
            details.JobTypes.Select(JobType.FromFullTypeName).ToArray(),
            details.Jobs.Select(JobListItem.FromJob).ToArray());
    }
}