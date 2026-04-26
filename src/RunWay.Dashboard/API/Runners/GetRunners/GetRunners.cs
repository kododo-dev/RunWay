using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Runners.GetRunners;

internal sealed record GetRunners : IRequest<IReadOnlyCollection<RunnerListItem>>;