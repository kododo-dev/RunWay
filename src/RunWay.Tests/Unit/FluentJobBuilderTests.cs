using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Schedule;
using NSubstitute;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

/// <summary>
/// The chains from the README, written the same way, through the public <see cref="IScheduler"/>:
/// if a setting stops returning the builder that has ScheduleAsync / SetRecurrenceAsync, this file no longer compiles.
/// </summary>
public class FluentJobBuilderTests
{
    private sealed class SendEmailJob
    {
        public required string To { get; set; }
        public required string Subject { get; set; }
    }

    private readonly IStore _store = Substitute.For<IStore>();
    private readonly IRecurrenceCalculator _calculator = Substitute.For<IRecurrenceCalculator>();
    private readonly IScheduler _scheduler;

    public FluentJobBuilderTests()
    {
        _store.CreateAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<Job>()!);
        _store.CreateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<Job>()!);
        _store.FindRecurrenceAsync(Arg.Any<RecurrenceId>(), Arg.Any<CancellationToken>()).Returns((Recurrence?)null);
        _store.CreateRecurrenceWithoutTransactionAsync(Arg.Any<Recurrence>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<Recurrence>()!);
        _store.UpdateRecurrenceWithoutTransactionAsync(Arg.Any<Recurrence>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<Recurrence>()!);
        _calculator.CalculateNextOccurrence(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(DateTimeOffset.UtcNow);
        _scheduler = new Scheduler(_store, _calculator, new RunWayOptions());
    }

    [Fact]
    public async Task Readme_SchedulingOptions_ChainEndsWithScheduleAsync()
    {
        // README, "Scheduling options"
        await _scheduler
            .Job(new SendEmailJob { To = "user@example.com", Subject = "Hello" })
            .WithPriority(10)
            .WithRetryDelaysInSeconds(10, 60, 300)
            .WithTimeout(TimeSpan.FromMinutes(5))
            .ScheduleAsync(CancellationToken.None);

        await _store.Received(1).CreateAsync(
            Arg.Is<Job>(j => j != null && j.Options.Priority == 10
                && j.Options.RetryDelaysInSeconds.SequenceEqual(new[] { 10, 60, 300 })
                && j.Options.Timeout == TimeSpan.FromMinutes(5)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Readme_Outbox_AsTransactionalFalse_UsesAmbientTransaction()
    {
        // README, "Outbox pattern"
        await _scheduler
            .Job(new SendEmailJob { To = "user@example.com", Subject = "Order confirmed" })
            .AsTransactional(false)
            .ScheduleAsync(CancellationToken.None);

        await _store.Received(1).CreateWithoutTransactionAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>());
        await _store.DidNotReceive().CreateAsync(Arg.Any<Job>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChainWithScheduledTime_EndsWithScheduleAsync()
    {
        var at = new DateTimeOffset(2030, 1, 1, 8, 0, 0, TimeSpan.Zero);

        await _scheduler.Job(new SendEmailJob { To = "a@b.c", Subject = "Later" })
            .AsTransactional(false)
            .WithPriority(3)
            .ScheduleAsync(at, CancellationToken.None);

        await _store.Received(1).CreateWithoutTransactionAsync(
            Arg.Is<Job>(j => j != null && j.ScheduledAt == at && j.Options.Priority == 3), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecurrenceChain_EndsWithSetRecurrenceAsync()
    {
        await _scheduler.Recurrence(new SendEmailJob { To = "a@b.c", Subject = "Report" })
            .WithTimeout(TimeSpan.FromMinutes(30))
            .WithRetryDelaysInSeconds(60, 300)
            .SetRecurrenceAsync("nightly-report", "0 2 * * *", CancellationToken.None);

        await _store.Received(1).CreateRecurrenceWithoutTransactionAsync(
            Arg.Is<Recurrence>(r => r != null && r.Options.Timeout == TimeSpan.FromMinutes(30)
                && r.Options.RetryDelaysInSeconds.SequenceEqual(new[] { 60, 300 })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SettingsThroughTheBaseInterface_StillApply()
    {
        // Code written against IJobBuilder<T> (e.g. the SetRecurrenceAsync configure callback) keeps working.
        var builder = _scheduler.Job(new SendEmailJob { To = "a@b.c", Subject = "Base" });
        IJobBuilder<SendEmailJob> configure = builder;
        configure.WithPriority(7).WithTimeout(TimeSpan.FromSeconds(30));

        await builder.ScheduleAsync(CancellationToken.None);

        await _store.Received(1).CreateAsync(
            Arg.Is<Job>(j => j != null && j.Options.Priority == 7 && j.Options.Timeout == TimeSpan.FromSeconds(30)), Arg.Any<CancellationToken>());
    }
}
