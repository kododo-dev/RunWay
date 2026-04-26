using FluentAssertions;
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
        result.Should().BeAfter(from);
    }

    [Fact]
    public void CalculateNextOccurrence_WithStepExpression_ReturnsFutureDate()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        result.Should().BeAfter(from);
    }

    [Fact]
    public void CalculateNextOccurrence_WithStepExpression_ReturnsCorrectInterval()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        result.Minute.Should().Be(5);
    }

    [Fact]
    public void CalculateNextOccurrence_WithMidMinute_ReturnsNextSlot()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 3, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        result.Minute.Should().Be(5);
    }

    [Fact]
    public void CalculateNextOccurrence_WithHourlyExpression_AdvancesHour()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("0 * * * *", from);
        result.Hour.Should().Be(13);
        result.Minute.Should().Be(0);
    }

    [Fact]
    public void CalculateNextOccurrence_WithInvalidExpression_Throws()
    {
        var from = DateTimeOffset.UtcNow;
        var act = () => _calculator.CalculateNextOccurrence("not a cron", from);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void CalculateNextOccurrence_ResultIsUtc()
    {
        var from = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var result = _calculator.CalculateNextOccurrence("*/5 * * * *", from);
        result.Offset.Should().Be(TimeSpan.Zero);
    }
}
