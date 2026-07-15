using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Schedule;
using NSubstitute;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

public class RecurringJobBuilderTests
{
    private readonly IStore _store = Substitute.For<IStore>();
    private readonly IRecurrenceCalculator _calculator = Substitute.For<IRecurrenceCalculator>();
    private readonly RunWayOptions _options = new();
    private readonly DateTimeOffset _nextOccurrence = new(2024, 1, 1, 12, 5, 0, TimeSpan.Zero);

    private RecurringJobBuilder<object> CreateBuilder(object data = null!)
    {
        var builder = new RecurringJobBuilder<object>(data, _store, _calculator, _options);
        _calculator.CalculateNextOccurrence(Arg.Any<string>(), Arg.Any<DateTimeOffset>())
            .Returns(_nextOccurrence);
        _store.FindRecurrenceAsync(Arg.Any<RecurrenceId>(), Arg.Any<CancellationToken>())
            .Returns((Recurrence?)null);
        _store.CreateRecurrenceWithoutTransactionAsync(Arg.Any<Recurrence>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Recurrence>()!);
        _store.CreateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Job>()!);
        _store.UpdateRecurrenceWithoutTransactionAsync(Arg.Any<Recurrence>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Recurrence>()!);
        return builder;
    }

    [Fact]
    public async Task SetRecurrenceAsync_NewRecurrence_StoresExactCronExpression()
    {
        var builder = CreateBuilder();
        const string expression = "0 * * * *";

        await builder.SetRecurrenceAsync("test-key", expression, CancellationToken.None);

        await _store.Received(1).CreateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Rule == expression),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRecurrenceAsync_WithStepExpression_StoresShorthandNotExpanded()
    {
        var builder = CreateBuilder();
        const string expression = "*/5 * * * *";

        await builder.SetRecurrenceAsync("test-key", expression, CancellationToken.None);

        await _store.Received(1).CreateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Rule == "*/5 * * * *"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRecurrenceAsync_WithStepExpression_DoesNotStoreExpandedForm()
    {
        var builder = CreateBuilder();
        const string expression = "*/5 * * * *";

        await builder.SetRecurrenceAsync("test-key", expression, CancellationToken.None);

        await _store.Received(1).CreateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Rule != "0,5,10,15,20,25,30,35,40,45,50,55 * * * *"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRecurrenceAsync_NewRecurrence_CreatesJob()
    {
        var builder = CreateBuilder();

        await builder.SetRecurrenceAsync("test-key", "0 * * * *", CancellationToken.None);

        await _store.Received(1).CreateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRecurrenceAsync_NewRecurrence_CalculatesNextOccurrence()
    {
        var builder = CreateBuilder();
        const string expression = "*/15 * * * *";

        await builder.SetRecurrenceAsync("test-key", expression, CancellationToken.None);

        _calculator.Received(1).CalculateNextOccurrence(expression, Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task SetRecurrenceAsync_ExistingRecurrenceWithSameRule_DoesNotUpdate()
    {
        const string expression = "0 * * * *";
        var existingData = _options.JobDataSerializer.Serialize(new { });
        var existingOptions = new JobOptions(0, [], TimeSpan.FromMinutes(1));
        var existing = Recurrence.CreateNew(new RecurrenceId("test-key"), expression, existingData, existingOptions);

        _store.FindRecurrenceAsync(Arg.Any<RecurrenceId>(), Arg.Any<CancellationToken>())
            .Returns(existing);
        _calculator.CalculateNextOccurrence(Arg.Any<string>(), Arg.Any<DateTimeOffset>())
            .Returns(_nextOccurrence);

        var builder = new RecurringJobBuilder<object>(new { }, _store, _calculator, _options);

        await builder.SetRecurrenceAsync("test-key", expression, CancellationToken.None);

        await _store.DidNotReceive().UpdateRecurrenceWithoutTransactionAsync(Arg.Any<Recurrence>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetRecurrenceAsync_ExistingRecurrenceWithDifferentRule_Updates()
    {
        const string oldExpression = "0 * * * *";
        const string newExpression = "*/5 * * * *";
        var existingData = _options.JobDataSerializer.Serialize(new { });
        var existingOptions = new JobOptions(0, [], TimeSpan.FromMinutes(1));
        var existing = Recurrence.CreateNew(new RecurrenceId("test-key"), oldExpression, existingData, existingOptions);

        _store.FindRecurrenceAsync(Arg.Any<RecurrenceId>(), Arg.Any<CancellationToken>())
            .Returns(existing);
        _calculator.CalculateNextOccurrence(Arg.Any<string>(), Arg.Any<DateTimeOffset>())
            .Returns(_nextOccurrence);
        _store.CreateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Job>()!);
        _store.UpdateRecurrenceWithoutTransactionAsync(Arg.Any<Recurrence>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Recurrence>()!);

        var builder = new RecurringJobBuilder<object>(new { }, _store, _calculator, _options);

        await builder.SetRecurrenceAsync("test-key", newExpression, CancellationToken.None);

        await _store.Received(1).UpdateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Rule == newExpression),
            Arg.Any<CancellationToken>());
    }
}
