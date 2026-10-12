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
        Assert.Equal(JobStatus.Scheduled, job.Status);
    }

    [Fact]
    public void CreateNew_RunnerId_IsNull()
    {
        var job = NewScheduled();
        Assert.Null(job.RunnerId);
    }

    [Fact]
    public void CreateNew_AddsPendingCreatedAndScheduledAuditEvents()
    {
        var job = NewScheduled();
        Assert.Equal(
            new[] { JobAuditRecordType.Created, JobAuditRecordType.Scheduled }.Order(),
            job.PendingEvents.Select(e => e.Type).Order());
    }

    [Fact]
    public void Started_SetsStatusToRunning()
    {
        var job = NewScheduled();
        job.Started(SampleRunner);
        Assert.Equal(JobStatus.Running, job.Status);
    }

    [Fact]
    public void Started_SetsRunnerId()
    {
        var job = NewScheduled();
        job.Started(SampleRunner);
        Assert.Equal(SampleRunner.Id, job.RunnerId);
    }

    [Fact]
    public void Started_AddsStartedAuditEvent()
    {
        var job = NewScheduled();
        job.Started(SampleRunner);
        Assert.Contains(job.PendingEvents, e => e.Type == JobAuditRecordType.Started);
    }

    [Fact]
    public void Started_WhenAlreadyRunning_Throws()
    {
        var job = NewRunning();
        var act = () => job.Started(SampleRunner);
        Assert.ThrowsAny<InvalidOperationException>(act);
    }

    [Fact]
    public void Started_WhenJobHasRunnerId_Throws()
    {
        var job = Job.CreateNew(SampleData, JobOptions.Default, DateTimeOffset.UtcNow, null);
        job.Started(SampleRunner);

        var act = () => job.Started(SampleRunner);
        Assert.ThrowsAny<InvalidOperationException>(act);
    }

    [Fact]
    public void Succeeded_SetsStatusToSucceeded()
    {
        var job = NewRunning();
        job.Succeeded();
        Assert.Equal(JobStatus.Succeeded, job.Status);
    }

    [Fact]
    public void Succeeded_ClearsRunnerId()
    {
        var job = NewRunning();
        job.Succeeded();
        Assert.Null(job.RunnerId);
    }

    [Fact]
    public void Succeeded_AddsSucceededAuditEvent()
    {
        var job = NewRunning();
        job.Succeeded();
        Assert.Contains(job.PendingEvents, e => e.Type == JobAuditRecordType.Succeeded);
    }

    [Fact]
    public void Succeeded_WhenNotRunning_Throws()
    {
        var job = NewScheduled();
        var act = () => job.Succeeded();
        Assert.ThrowsAny<InvalidOperationException>(act);
    }

    [Fact]
    public void Failed_WhenNoRetriesLeft_SetsStatusToFailed()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        Assert.Equal(JobStatus.Failed, job.Status);
    }

    [Fact]
    public void Failed_WhenNoRetriesLeft_ClearsRunnerId()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        Assert.Null(job.RunnerId);
    }

    [Fact]
    public void Failed_WhenRetriesLeft_ReschedulesJob()
    {
        var job = NewRunning(retryDelays: [10, 60]);
        job.Failed("error");
        Assert.Equal(JobStatus.Scheduled, job.Status);
    }

    [Fact]
    public void Failed_WhenRetriesLeft_IncrementsRetriesCount()
    {
        var job = NewRunning(retryDelays: [10, 60]);
        job.Failed("error");
        Assert.Equal(1, job.RetriesCount);
    }

    [Fact]
    public void Failed_WhenRetriesLeft_AddsScheduledAuditEvent()
    {
        var job = NewRunning(retryDelays: [10]);
        job.Failed("error");
        Assert.Contains(job.PendingEvents, e => e.Type == JobAuditRecordType.Scheduled);
    }

    [Fact]
    public void Failed_WhenAllRetriesExhausted_SetsStatusToFailed()
    {
        var job = NewRunning(retryDelays: [10]);
        job.Failed("first failure");

        job.Started(SampleRunner);
        job.Failed("second failure");

        Assert.Equal(JobStatus.Failed, job.Status);
    }

    [Fact]
    public void Failed_WhenNotRunning_Throws()
    {
        var job = NewScheduled();
        var act = () => job.Failed("error");
        Assert.ThrowsAny<InvalidOperationException>(act);
    }

    [Fact]
    public void Requeue_SetsStatusToScheduled()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.Requeue();
        Assert.Equal(JobStatus.Scheduled, job.Status);
    }

    [Fact]
    public void Requeue_IncrementsRetriesCount()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        var countBefore = job.RetriesCount;
        job.Requeue();
        Assert.Equal(countBefore + 1, job.RetriesCount);
    }

    [Fact]
    public void Requeue_ClearsRunnerId()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.Requeue();
        Assert.Null(job.RunnerId);
    }

    [Fact]
    public void Requeue_AddsScheduledAuditEvent()
    {
        var job = NewRunning(retryDelays: []);
        job.Failed("error");
        job.Requeue();
        Assert.Contains(job.PendingEvents, e => e.Type == JobAuditRecordType.Scheduled);
    }

    [Fact]
    public void Requeue_WhenNotFailed_Throws()
    {
        var job = NewScheduled();
        var act = () => job.Requeue();
        Assert.ThrowsAny<InvalidOperationException>(act);
    }

    [Fact]
    public void Requeue_WhenRunning_Throws()
    {
        var job = NewRunning();
        var act = () => job.Requeue();
        Assert.ThrowsAny<InvalidOperationException>(act);
    }
}
