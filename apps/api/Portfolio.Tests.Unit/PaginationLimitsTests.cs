using Portfolio.Application.Common;
using Xunit;

namespace Portfolio.Tests.Unit;

public class PaginationLimitsTests
{
    [Theory]
    [InlineData(10, 10)]
    [InlineData(1, 1)]
    [InlineData(48, 48)]
    public void ClampPageSize_WithinBounds_ReturnsRequestedValue(int requested, int expected)
    {
        Assert.Equal(expected, PaginationLimits.ClampPageSize(requested));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ClampPageSize_NonPositive_ReturnsDefault(int requested)
    {
        Assert.Equal(PaginationLimits.DefaultPageSize, PaginationLimits.ClampPageSize(requested));
    }

    [Theory]
    [InlineData(49)]
    [InlineData(999999999)]
    [InlineData(int.MaxValue)]
    public void ClampPageSize_AboveMax_IsCappedAtMax(int requested)
    {
        // This is the specific case the spec calls out by name:
        // ?pageSize=999999999 must never reach the database as-is.
        Assert.Equal(PaginationLimits.MaxPageSize, PaginationLimits.ClampPageSize(requested));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ClampPage_NonPositive_ReturnsPageOne(int requested)
    {
        Assert.Equal(1, PaginationLimits.ClampPage(requested));
    }

    [Fact]
    public void ClampPage_PositiveValue_IsUnchanged()
    {
        Assert.Equal(7, PaginationLimits.ClampPage(7));
    }
}
