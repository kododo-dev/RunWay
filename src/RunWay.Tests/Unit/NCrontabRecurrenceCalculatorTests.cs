using Xunit;

namespace Kododo.RunWay.Tests.Unit;

public class NCrontabRecurrenceCalculatorTests
{
    private readonly NCrontabRecurrenceCalculator _calculator = new();

    [Fact]
    public void CalculateNextOccurrence_WithStandardExpression_ReturnsFutureDate()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("0 * * * *", from);
        Assert.True(result > from, $"Expected {result:O} to be after {from:O}.");
    }

    [Fact]
    public void CalculateNextOccurrence_WithStepExpression_ReturnsFutureDate()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        Assert.True(result > from, $"Expected {result:O} to be after {from:O}.");
    }

    [Fact]
    public void CalculateNextOccurrence_WithStepExpression_ReturnsCorrectInterval()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        Assert.Equal(5, result.Minute);
    }

    [Fact]
    public void CalculateNextOccurrence_WithMidMinute_ReturnsNextSlot()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 3, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        Assert.Equal(5, result.Minute);
    }

    [Fact]
    public void CalculateNextOccurrence_WithHourlyExpression_AdvancesHour()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("0 * * * *", from);
        Assert.Equal(13, result.Hour);
        Assert.Equal(0, result.Minute);
    }

    [Fact]
    public void CalculateNextOccurrence_WithInvalidExpression_Throws()
    {
        var from = DateTimeOffset.UtcNow;
        var act = () => _calculator.CalculateNextOccurrence("not a cron", from);
        Assert.ThrowsAny<Exception>(() => act());
    }

    [Fact]
    public void CalculateNextOccurrence_ResultIsUtc()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }
}
