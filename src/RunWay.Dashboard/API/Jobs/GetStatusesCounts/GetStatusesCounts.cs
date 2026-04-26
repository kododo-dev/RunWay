using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetStatusesCounts;

internal sealed record GetStatusesCounts : IRequest<StatusesCountsResult>;