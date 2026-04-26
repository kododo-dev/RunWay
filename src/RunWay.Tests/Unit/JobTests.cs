using FluentAssertions;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Jobs.Audit;
using Kododo.RunWay.Core.Runners;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

public class JobTests
{
    private static readonly JobData  SampleData    = new("TestType", "{}");
    private static readonly RunnerInfo SampleRunner = new(new RunnerId("1"), "test-runner");

    private static Job NewScheduled(IReadOnlyList<int>? retryDelays = null) =>
        Job.CreateNew(SampleData, new JobOptions(0, retryDelays ?? [], TimeSpan.FromMinutes(1)), DateTimeOffset.UtcNow, null);

    private static Job NewRunning(IReadOnlyList<int>? retryDelays = null)
    {
        var job = NewScheduled(retryDelays);
        job.Started(SampleRunner);
        return job;
    }

    [Fact]
    public void CreateNew_SetsStatusToScheduled()
    {
        var job = NewScheduled();
        job.Status.Should().Be(JobStatus.Scheduled);
    }

    [Fact]
    public void CreateNew_RunnerId_IsNull()
    {
        var job = NewScheduled();
        job.RunnerId.Should().BeNull();
    }

    [Fact]
    public void CreateNew_AddsPendingCreatedAndScheduledAuditEvents()
    {
        var job = NewScheduled();
        job.PendingEvents.Select(e => e.Type)
            .Should().BeEquivalentTo([JobAuditRecordType.Created, JobAuditRecordType.Scheduled]);
    }

    [Fact]
    public void Started_SetsStatusToRunning()
    {
        var job = NewScheduled();
        job.Started(SampleRunner);
        job.Status.Should().Be(JobStatus.Running);
    }

    [Fact]
    public void Started_SetsRunnerId()
    {
        var job = NewScheduled();
        job.Started(SampleRunner);
        job.RunnerId.Should().Be(SampleRunner.Id);
    }

    [Fact]
    public void Started_AddsStartedAuditEvent()
    {
        var job = NewScheduled();
        job.Started(SampleRunner);
        job.PendingEvents.Should().Contain(e => e.Type == JobAuditRecordType.Started);
    }

    [Fact]
    public void Started_WhenAlreadyRunning_Throws()
    {
        var job = NewRunning();
        var act = () => job.Started(SampleRunner);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Started_WhenJobHasRunnerId_Throws()
    {
        var job = Job.CreateNew(SampleData, JobOptions.Default, DateTimeOffset.UtcNow, null);
        job.Started(SampleRunner);

        var act = () => job.Started(SampleRunner);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Succeeded_SetsStatusToSucceeded()
    {
        var job = NewRunning();
        job.Succeeded();
        job.Status.Should().Be(JobStatus.Succeeded);
    }

    [Fact]
    public void Succeeded_ClearsRunnerId()
    {
        var job = NewRunning();
        job.Succeeded();
        job.RunnerId.Should().BeNull();
    }

    [Fact]
    public void Succeeded_AddsSucceededAuditEvent()
    {
        var job = NewRunning();
        job.Succeeded();
        job.PendingEvents.Should().Contain(e => e.Type == JobAuditRecordType.Succeeded);
    }

    [Fact]
    public void Succeeded_WhenNotRunning_Throws()
    {
        var job = NewScheduled();
        var act = () => job.Succeeded();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failed_WhenNoRetriesLeft_SetsStatusToFailed()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.Status.Should().Be(JobStatus.Failed);
    }

    [Fact]
    public void Failed_WhenNoRetriesLeft_ClearsRunnerId()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.RunnerId.Should().BeNull();
    }

    [Fact]
    public void Failed_WhenRetriesLeft_ReschedulesJob()
    {
        var job = NewRunning(retryDelays: [10, 60]);
        job.Failed("error");
        job.Status.Should().Be(JobStatus.Scheduled);
    }

    [Fact]
    public void Failed_WhenRetriesLeft_IncrementsRetriesCount()
    {
        var job = NewRunning(retryDelays: [10, 60]);
        job.Failed("error");
        job.RetriesCount.Should().Be(1);
    }

    [Fact]
    public void Failed_WhenRetriesLeft_AddsScheduledAuditEvent()
    {
        var job = NewRunning(retryDelays: [10]);
        job.Failed("error");
        job.PendingEvents.Should().Contain(e => e.Type == JobAuditRecordType.Scheduled);
    }

    [Fact]
    public void Failed_WhenAllRetriesExhausted_SetsStatusToFailed()
    {
        var job = NewRunning(retryDelays: [10]);
        job.Failed("first failure");

        job.Started(SampleRunner);
        job.Failed("second failure");

        job.Status.Should().Be(JobStatus.Failed);
    }

    [Fact]
    public void Failed_WhenNotRunning_Throws()
    {
        var job = NewScheduled();
        var act = () => job.Failed("error");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Requeue_SetsStatusToScheduled()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.Requeue();
        job.Status.Should().Be(JobStatus.Scheduled);
    }

    [Fact]
    public void Requeue_IncrementsRetriesCount()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        var countBefore = job.RetriesCount;
        job.Requeue();
        job.RetriesCount.Should().Be(countBefore + 1);
    }

    [Fact]
    public void Requeue_ClearsRunnerId()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.Requeue();
        job.RunnerId.Should().BeNull();
    }

    [Fact]
    public void Requeue_AddsScheduledAuditEvent()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.Requeue();
        job.PendingEvents.Should().Contain(e => e.Type == JobAuditRecordType.Scheduled);
    }

    [Fact]
    public void Requeue_WhenNotFailed_Throws()
    {
        var job = NewScheduled();
        var act = () => job.Requeue();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Requeue_WhenRunning_Throws()
    {
        var job = NewRunning();
        var act = () => job.Requeue();
        act.Should().Throw<InvalidOperationException>();
    }
}
