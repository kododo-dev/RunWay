using Kododo.RunWay.Core.Store.Queries;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

public class PaginationTests
{
    [Fact]
    public void Pagination_WhenPageIsZero_Throws()
    {
        var act = () => new Pagination(0);
        Assert.ThrowsAny<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Pagination_WhenPageIsNegative_Throws()
    {
        var act = () => new Pagination(-1);
        Assert.ThrowsAny<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Pagination_WhenPageSizeIsZero_Throws()
    {
        var act = () => new Pagination(1, 0);
        Assert.ThrowsAny<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Pagination_WhenPageSizeIsNegative_Throws()
    {
        var act = () => new Pagination(1, -5);
        Assert.ThrowsAny<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Pagination_WhenValid_DoesNotThrow()
    {
        var act = () => new Pagination(1);
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void Pagination_WhenValid_SetsProperties()
    {
        var pagination = new Pagination(3, 50);
        Assert.Equal(3, pagination.Page);
        Assert.Equal(50, pagination.PageSize);
    }

    [Fact]
    public void Pagination_DefaultPageSize_IsTwenty()
    {
        var pagination = new Pagination(1);
        Assert.Equal(20, pagination.PageSize);
    }
}
