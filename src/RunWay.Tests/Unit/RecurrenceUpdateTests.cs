using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Schedule;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

/// <summary>
/// SetRecurrenceAsync on an existing recurrence: a change of the data, the options or the expression is saved, and
/// the job waiting for the next occurrence gets it, without a second job.
/// </summary>
public class RecurrenceUpdateTests
{
    private const string Key = "report";
    private const string Cron = "0 2 * * *";
    private static readonly DateTimeOffset WaitingAt = new(2030, 1, 1, 2, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset NextOccurrence = new(2030, 1, 1, 5, 0, 0, TimeSpan.Zero);

    private sealed class ReportJob
    {
        public string Kind { get; set; } = "";
    }

    private readonly IStore _store = Substitute.For<IStore>();
    private readonly IRecurrenceCalculator _calculator = Substitute.For<IRecurrenceCalculator>();
    private readonly RunWayOptions _options = new();

    public RecurrenceUpdateTests()
    {
        _calculator.CalculateNextOccurrence(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(NextOccurrence);
        _store.UpdateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<Job>()!);
        _store.CreateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<Job>()!);
        _store.UpdateRecurrenceWithoutTransactionAsync(Arg.Any<Recurrence>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<Recurrence>()!);
    }

    private JobData Data(string kind) => _options.JobDataSerializer.Serialize(new ReportJob { Kind = kind });

    private static JobOptions Options(int priority = 0, int[]? delays = null) =>
        new(priority, delays ?? [0, 10, 60, 600], TimeSpan.FromMinutes(10));

    private static Job ExistingJob(JobData data, JobOptions options, JobStatus status = JobStatus.Scheduled, RunnerId? runner = null) =>
        new(new JobId("1"), 3, data, options, new RecurrenceId(Key), status, runner, DateTimeOffset.UtcNow, WaitingAt, 0);

    /// <summary>The recurrence as read back from the store: new instances, equal values.</summary>
    private void Stored(string rule, JobData data, JobOptions options, Job? currentJob) =>
        _store.FindRecurrenceAsync(Arg.Any<RecurrenceId>(), Arg.Any<CancellationToken>())
            .Returns(new Recurrence(new RecurrenceId(Key), 2, rule, data with { }, options with { RetryDelaysInSeconds = options.RetryDelaysInSeconds.ToArray() }, currentJob));

    private Task SetAsync(string cron, string kind, Action<IRecurringJobBuilder<ReportJob>>? configure = null)
    {
        var builder = new Scheduler(_store, _calculator, _options).Recurrence(new ReportJob { Kind = kind });
        configure?.Invoke(builder);
        return builder.SetRecurrenceAsync(Key, cron, CancellationToken.None);
    }

    [Fact]
    public void JobOptions_WithEqualDelaysInDifferentLists_AreEqual()
    {
        var a = Options(1, [5, 10]);
        var b = Options(1, [5, 10]);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, Options(1, [5, 11]));
    }

    [Fact]
    public async Task Unchanged_ReadBackFromStore_WritesNothing()
    {
        Stored(Cron, Data("daily"), Options(), ExistingJob(Data("daily"), Options()));

        await SetAsync(Cron, "daily");

        await _store.DidNotReceiveWithAnyArgs().UpdateRecurrenceWithoutTransactionAsync(default!, default);
        await _store.DidNotReceiveWithAnyArgs().UpdateWithoutTransactionAsync(default!, default);
        await _store.DidNotReceiveWithAnyArgs().CreateWithoutTransactionAsync(default!, default);
    }

    [Fact]
    public async Task DataChanged_SameExpression_UpdatesRecurrenceAndWaitingJob()
    {
        Stored(Cron, Data("daily"), Options(), ExistingJob(Data("daily"), Options()));

        await SetAsync(Cron, "weekly");

        await _store.Received(1).UpdateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Data == Data("weekly") && r.CurrentJob!.Id == new JobId("1")), Arg.Any<CancellationToken>());
        await _store.Received(1).UpdateWithoutTransactionAsync(
            Arg.Is<Job>(j => j != null && j.Data == Data("weekly") && j.ScheduledAt == WaitingAt), Arg.Any<CancellationToken>());
        await _store.DidNotReceiveWithAnyArgs().CreateWithoutTransactionAsync(default!, default);
    }

    [Fact]
    public async Task OptionsChanged_SameExpression_UpdatesRecurrenceAndWaitingJob()
    {
        Stored(Cron, Data("daily"), Options(), ExistingJob(Data("daily"), Options()));

        await SetAsync(Cron, "daily", b => b.WithPriority(5).WithRetryDelaysInSeconds(30));

        await _store.Received(1).UpdateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Options.Priority == 5), Arg.Any<CancellationToken>());
        await _store.Received(1).UpdateWithoutTransactionAsync(
            Arg.Is<Job>(j => j != null && j.Options.Priority == 5 && j.Options.RetryDelaysInSeconds.SequenceEqual(new[] { 30 })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExpressionChanged_MovesWaitingJob_InsteadOfAddingASecondOne()
    {
        Stored(Cron, Data("daily"), Options(), ExistingJob(Data("daily"), Options()));

        await SetAsync("0 5 * * *", "daily");

        await _store.Received(1).UpdateWithoutTransactionAsync(
            Arg.Is<Job>(j => j != null && j.ScheduledAt == NextOccurrence), Arg.Any<CancellationToken>());
        await _store.DidNotReceiveWithAnyArgs().CreateWithoutTransactionAsync(default!, default);
        await _store.Received(1).UpdateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Rule == "0 5 * * *"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JobRunning_UpdatesOnlyRecurrence_NextJobUsesNewSettings()
    {
        var running = ExistingJob(Data("daily"), Options(), JobStatus.Running, new RunnerId("7"));
        Stored(Cron, Data("daily"), Options(), running);

        await SetAsync(Cron, "weekly");

        await _store.Received(1).UpdateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Data == Data("weekly") && r.CurrentJob!.Id == running.Id), Arg.Any<CancellationToken>());
        await _store.DidNotReceiveWithAnyArgs().UpdateWithoutTransactionAsync(default!, default);
        await _store.DidNotReceiveWithAnyArgs().CreateWithoutTransactionAsync(default!, default);
    }

    [Fact]
    public async Task JobCompleted_CreatesNextJobWithNewSettings()
    {
        Stored(Cron, Data("daily"), Options(), ExistingJob(Data("daily"), Options(), JobStatus.Succeeded));

        await SetAsync(Cron, "weekly");

        await _store.Received(1).CreateWithoutTransactionAsync(
            Arg.Is<Job>(j => j != null && j.Data == Data("weekly") && j.ScheduledAt == NextOccurrence), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunnerTookTheJobMeanwhile_RecurrenceIsStillUpdated()
    {
        Stored(Cron, Data("daily"), Options(), ExistingJob(Data("daily"), Options()));
        var conflict = new InvalidOperationException("The job has a newer version");
        _store.UpdateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>()).ThrowsAsync(conflict);
        _store.IsConcurrencyException(conflict).Returns(true);

        await SetAsync(Cron, "weekly");

        await _store.Received(1).UpdateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Data == Data("weekly")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Reconfigure_RunningJob_Throws()
    {
        var running = ExistingJob(Data("daily"), Options(), JobStatus.Running, new RunnerId("7"));

        Assert.Throws<InvalidOperationException>(() => running.Reconfigure(Data("weekly"), Options(), WaitingAt));
    }
}
