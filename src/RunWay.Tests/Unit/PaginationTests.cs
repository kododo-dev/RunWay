using FluentAssertions;
using Kododo.RunWay.Core.Store.Queries;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

public class PaginationTests
{
    [Fact]
    public void Pagination_WhenPageIsZero_Throws()
    {
        var act = () => new Pagination(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Pagination_WhenPageIsNegative_Throws()
    {
        var act = () => new Pagination(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Pagination_WhenPageSizeIsZero_Throws()
    {
        var act = () => new Pagination(1, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Pagination_WhenPageSizeIsNegative_Throws()
    {
        var act = () => new Pagination(1, -5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Pagination_WhenValid_DoesNotThrow()
    {
        var act = () => new Pagination(1);
        act.Should().NotThrow();
    }

    [Fact]
    public void Pagination_WhenValid_SetsProperties()
    {
        var pagination = new Pagination(3, 50);
        pagination.Page.Should().Be(3);
        pagination.PageSize.Should().Be(50);
    }

    [Fact]
    public void Pagination_DefaultPageSize_IsTwenty()
    {
        var pagination = new Pagination(1);
        pagination.PageSize.Should().Be(20);
    }
}
