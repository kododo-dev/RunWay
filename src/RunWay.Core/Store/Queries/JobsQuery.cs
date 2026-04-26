namespace Kododo.RunWay.Core.Store.Queries;

public sealed record JobsQuery : JobQuery
{
    public required Pagination Pagination { get; set; }
}