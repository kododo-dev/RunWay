using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Recurrences.GetRecurrenceJobs;

internal sealed record GetRecurrenceJobs(string Id, int Page) : IRequest<PagedResult<JobListItem>>;