namespace Kododo.RunWay.Dashboard.API;

internal sealed record PagedResult<T>(T[] Items, int TotalItems, int TotalPages, int PageSize);