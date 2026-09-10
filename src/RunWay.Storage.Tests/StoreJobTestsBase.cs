using FluentAssertions;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Jobs.Audit;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kododo.RunWay.Storage.Tests;

public abstract class StoreJobTestsBase<TFixture> : StoreTestBase<TFixture>
    where TFixture : class, IStorageTestFixture
{
    private RunnerInfo _sampleRunner = null!;

    protected StoreJobTestsBase(TFixture fixture) : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _sampleRunner = await RegisterRunnerAsync("test-runner");
    }

    [Fact]
    public async Task CreateAsync_ReturnsNonEmptyJobId()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);
        job.Id.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_PersistedJobCanBeFound()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);
        var found = await Store.FindAsync(job.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found.Id.Should().Be(job.Id);
    }

    [Fact]
    public async Task CreateAsync_CreatesAuditRecords()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);
        var details = await Store.GetDetailsAsync(job.Id, CancellationToken.None);

        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Created);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Scheduled);
    }

    [Fact]
    public async Task CreateAsync_PersistsJobData()
    {
        var data = new JobData("MyJobType", """{"key":"value"}""");
        var job = await Store.CreateAsync(
            Job.CreateNew(data, JobOptions.Default, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var found = await Store.FindAsync(job.Id, CancellationToken.None);

        found!.Data.Type.Should().Be("MyJobType");
    }

    [Fact]
    public async Task FindAsync_WhenJobDoesNotExist_ReturnsNull()
    {
        var result = await Store.FindAsync(new JobId("999999"), CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ChangesJobStatus()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);

        job.Started(_sampleRunner);
        await Store.UpdateAsync(job, CancellationToken.None);

        var updated = await Store.FindAsync(job.Id, CancellationToken.None);
        updated!.Status.Should().Be(JobStatus.Running);
    }

    [Fact]
    public async Task UpdateAsync_SavesAuditRecords()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);

        job.Started(_sampleRunner);
        await Store.UpdateAsync(job, CancellationToken.None);

        var details = await Store.GetDetailsAsync(job.Id, CancellationToken.None);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Started);
    }

    [Fact]
    public async Task UpdateAsync_Started_Succeeded_StatusIsSucceeded()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);

        job.Started(_sampleRunner);
        await Store.UpdateAsync(job, CancellationToken.None);

        var running = (await Store.FindAsync(job.Id, CancellationToken.None))!;
        running.Succeeded();
        await Store.UpdateAsync(running, CancellationToken.None);

        var result = await Store.FindAsync(job.Id, CancellationToken.None);
        result!.Status.Should().Be(JobStatus.Succeeded);
    }

    [Fact]
    public async Task GetManyAsync_ReturnsPaginatedResults()
    {
        for (var i = 0; i < 5; i++)
            await Store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await Store.GetManyAsync(
            new JobsQuery { Pagination = new Pagination(1, 3) },
            CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalItems.Should().Be(5);
    }

    [Fact]
    public async Task GetManyAsync_SecondPage_ReturnsRemainingItems()
    {
        for (var i = 0; i < 5; i++)
            await Store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await Store.GetManyAsync(
            new JobsQuery { Pagination = new Pagination(2, 3) },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetManyAsync_FiltersByStatus()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);
        job.Started(_sampleRunner);
        await Store.UpdateAsync(job, CancellationToken.None);

        await Store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await Store.GetManyAsync(new JobsQuery
        {
            Pagination = new Pagination(1),
            Filter = new JobsFilter { Statuses = [JobStatus.Running] }
        }, CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items.Single().Status.Should().Be(JobStatus.Running);
    }

    [Fact]
    public async Task GetManyAsync_FiltersByMaxScheduledAt_ExcludesFuture()
    {
        await Store.CreateAsync(NewJob(scheduledAt: DateTimeOffset.UtcNow.AddHours(-1)), CancellationToken.None);
        await Store.CreateAsync(NewJob(scheduledAt: DateTimeOffset.UtcNow.AddHours(+1)), CancellationToken.None);

        var result = await Store.GetManyAsync(new JobsQuery
        {
            Pagination = new Pagination(1),
            Filter = new JobsFilter { MaxScheduledAt = DateTimeOffset.UtcNow }
        }, CancellationToken.None);

        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        await Store.CreateAsync(NewJob(), CancellationToken.None);
        await Store.CreateAsync(NewJob(), CancellationToken.None);

        var count = await Store.GetCountAsync(new JobsFilter(), CancellationToken.None);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetCountAsync_FiltersByStatus()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);
        job.Started(_sampleRunner);
        await Store.UpdateAsync(job, CancellationToken.None);

        await Store.CreateAsync(NewJob(), CancellationToken.None);

        var count = await Store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Scheduled] },
            CancellationToken.None);

        count.Should().Be(1);
    }

    [Fact]
    public async Task GetDetailsAsync_ReturnsJobWithFullAuditTrail()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);
        job.Started(_sampleRunner);
        await Store.UpdateAsync(job, CancellationToken.None);

        var details = await Store.GetDetailsAsync(job.Id, CancellationToken.None);

        details.Id.Should().Be(job.Id);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Created);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Scheduled);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Started);
    }

    [Fact]
    public async Task GetOneOrDefaultAsync_ReturnsJobMatchingFilter()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await Store.GetOneOrDefaultAsync(new JobQuery
        {
            Filter = new JobsFilter { Statuses = [JobStatus.Scheduled] }
        }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(job.Id);
    }

    [Fact]
    public async Task GetOneOrDefaultAsync_WhenNoMatch_ReturnsNull()
    {
        var result = await Store.GetOneOrDefaultAsync(new JobQuery
        {
            Filter = new JobsFilter { Statuses = [JobStatus.Failed] }
        }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Failed_WithRetries_ReschedulesJobInStore()
    {
        var job = await Store.CreateAsync(NewJob(retryDelays: [30]), CancellationToken.None);
        job.Started(_sampleRunner);
        await Store.UpdateAsync(job, CancellationToken.None);

        var running = (await Store.FindAsync(job.Id, CancellationToken.None))!;
        running.Failed("transient error");
        await Store.UpdateAsync(running, CancellationToken.None);

        var result = await Store.FindAsync(job.Id, CancellationToken.None);
        result!.Status.Should().Be(JobStatus.Scheduled);
        result.RetriesCount.Should().Be(1);
    }

    [Fact]
    public async Task DataPersistedAcrossStoreInstances()
    {
        var job = await Store.CreateAsync(NewJob(), CancellationToken.None);

        await using var freshScope = ServiceProvider.CreateAsyncScope();
        var freshStore = freshScope.ServiceProvider.GetRequiredService<IStore>();
        var result = await freshStore.FindAsync(job.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(job.Id);
    }
}
