using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Recurrences.GetRecurrences;

internal sealed record GetRecurrences(int Page) : IRequest<PagedResult<RecurrenceDto>>;