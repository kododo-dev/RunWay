namespace Kododo.RunWay.Core.Store.Queries;

public sealed record Pagination
{
    public int Page { get; }
    public int PageSize { get; }

    public Pagination(int page, int pageSize = 20)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "Page must be >= 1.");
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be >= 1.");
        Page = page;
        PageSize = pageSize;
    }
}