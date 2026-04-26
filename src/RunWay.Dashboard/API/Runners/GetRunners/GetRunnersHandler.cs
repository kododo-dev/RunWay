using Kododo.Reiho.AspNetCore.API;
using Kododo.RunWay.Core.Store;

namespace Kododo.RunWay.Dashboard.API.Runners.GetRunners;

internal sealed class GetRunnersHandler(IRunnerStore repository) : IRequestHandler<GetRunners, IReadOnlyCollection<RunnerListItem>>
{
    public async Task<IReadOnlyCollection<RunnerListItem>> HandleAsync(GetRunners request, CancellationToken cancellationToken)
    {
        var runners = await repository.GetAllRunnersAsync(cancellationToken);
        return runners
            .Select(x => new RunnerListItem(
                x.Id.Value,
                x.Name,
                x.Status,
                x.LastSeenAt.UtcDateTime))
            .ToArray();
    }
}