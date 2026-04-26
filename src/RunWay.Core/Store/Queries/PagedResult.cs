namespace Kododo.RunWay.Core.Store.Queries;

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalItems);