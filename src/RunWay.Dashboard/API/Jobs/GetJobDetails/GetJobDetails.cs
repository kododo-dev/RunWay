using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetJobDetails;

internal sealed record GetJobDetails(string Id) : IRequest<Details>;