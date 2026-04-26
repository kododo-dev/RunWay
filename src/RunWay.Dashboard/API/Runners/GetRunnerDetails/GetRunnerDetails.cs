using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Runners.GetRunnerDetails;

internal sealed record GetRunnerDetails(string Id) : IRequest<RunnerDetailsDto>;