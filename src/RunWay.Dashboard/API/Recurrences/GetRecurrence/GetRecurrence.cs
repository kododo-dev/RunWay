using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Recurrences.GetRecurrence;

internal sealed record GetRecurrence(string Id) : IRequest<RecurrenceDto>;