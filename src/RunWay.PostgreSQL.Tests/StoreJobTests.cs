using FluentAssertions;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Jobs.Audit;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Kododo.RunWay.PostgreSQL.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Kododo.RunWay.PostgreSQL.Tests;

[Collection("PostgreSQL")]
public class StoreJobTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private ServiceProvider            _serviceProvider = null!;
    private AsyncServiceScope          _scope;
    private IStore                     _store = null!;
    private RunnerInfo                 _sampleRunner = null!;

    public StoreJobTests(PostgreSqlFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddRunWay(config =>
        {
            config.UsePostgreSQL(_ => new NpgsqlConnection(_fixture.ConnectionString));
        });
        _serviceProvider = services.BuildServiceProvider();
        _scope           = _serviceProvider.CreateAsyncScope();
        _store           = _scope.ServiceProvider.GetRequiredService<IStore>();

        await _store.InitializeAsync(CancellationToken.None);
        await _fixture.ResetAsync();

        var runnerId = await _store.RegisterRunnerAsync("test-runner", ["TestJob"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        _sampleRunner = new RunnerInfo(runnerId, "test-runner");
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    private static Job NewJob(IReadOnlyList<int>? retryDelays = null, DateTimeOffset? scheduledAt = null) =>
        Job.CreateNew(
            new JobData("TestJob", "{}"),
            new JobOptions(0, retryDelays ?? [], TimeSpan.FromMinutes(5)),
            scheduledAt ?? DateTimeOffset.UtcNow);

    [Fact]
    public async Task CreateAsync_ReturnsNonEmptyJobId()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);
        job.Id.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_PersistedJobCanBeFound()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);
        var found = await _store.FindAsync(job.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found.Id.Should().Be(job.Id);
    }

    [Fact]
    public async Task CreateAsync_CreatesAuditRecords()
    {
        var job   = await _store.CreateAsync(NewJob(), CancellationToken.None);
        var details = await _store.GetDetailsAsync(job.Id, CancellationToken.None);

        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Created);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Scheduled);
    }

    [Fact]
    public async Task CreateAsync_PersistsJobData()
    {
        var data  = new JobData("MyJobType", """{"key":"value"}""");
        var job = await _store.CreateAsync(
            Job.CreateNew(data, JobOptions.Default, DateTimeOffset.UtcNow),
            CancellationToken.None);

        var found = await _store.FindAsync(job.Id, CancellationToken.None);

        found!.Data.Type.Should().Be("MyJobType");
    }

    [Fact]
    public async Task FindAsync_WhenJobDoesNotExist_ReturnsNull()
    {
        var result = await _store.FindAsync(new JobId("999999"), CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ChangesJobStatus()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);
        
        job.Started(_sampleRunner);
        await _store.UpdateAsync(job, CancellationToken.None);

        var updated = await _store.FindAsync(job.Id, CancellationToken.None);
        updated!.Status.Should().Be(JobStatus.Running);
    }

    [Fact]
    public async Task UpdateAsync_SavesAuditRecords()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);

        job.Started(_sampleRunner);
        await _store.UpdateAsync(job, CancellationToken.None);

        var details = await _store.GetDetailsAsync(job.Id, CancellationToken.None);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Started);
    }

    [Fact]
    public async Task UpdateAsync_Started_Succeeded_StatusIsSucceeded()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);
        
        job.Started(_sampleRunner);
        await _store.UpdateAsync(job, CancellationToken.None);

        var running = (await _store.FindAsync(job.Id, CancellationToken.None))!;
        running.Succeeded();
        await _store.UpdateAsync(running, CancellationToken.None);

        var result = await _store.FindAsync(job.Id, CancellationToken.None);
        result!.Status.Should().Be(JobStatus.Succeeded);
    }

    [Fact]
    public async Task GetManyAsync_ReturnsPaginatedResults()
    {
        for (var i = 0; i < 5; i++)
            await _store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await _store.GetManyAsync(
            new JobsQuery { Pagination = new Pagination(1, 3) },
            CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalItems.Should().Be(5);
    }

    [Fact]
    public async Task GetManyAsync_SecondPage_ReturnsRemainingItems()
    {
        for (var i = 0; i < 5; i++)
            await _store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await _store.GetManyAsync(
            new JobsQuery { Pagination = new Pagination(2, 3) },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetManyAsync_FiltersByStatus()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);
        job.Started(_sampleRunner);
        await _store.UpdateAsync(job, CancellationToken.None);

        await _store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await _store.GetManyAsync(new JobsQuery
        {
            Pagination = new Pagination(1),
            Filter     = new JobsFilter { Statuses = [JobStatus.Running] }
        }, CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items.Single().Status.Should().Be(JobStatus.Running);
    }

    [Fact]
    public async Task GetManyAsync_FiltersByMaxScheduledAt_ExcludesFuture()
    {
        await _store.CreateAsync(NewJob(scheduledAt: DateTimeOffset.UtcNow.AddHours(-1)), CancellationToken.None);
        await _store.CreateAsync(NewJob(scheduledAt: DateTimeOffset.UtcNow.AddHours(+1)), CancellationToken.None);

        var result = await _store.GetManyAsync(new JobsQuery
        {
            Pagination = new Pagination(1),
            Filter     = new JobsFilter { MaxScheduledAt = DateTimeOffset.UtcNow }
        }, CancellationToken.None);

        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        await _store.CreateAsync(NewJob(), CancellationToken.None);
        await _store.CreateAsync(NewJob(), CancellationToken.None);

        var count = await _store.GetCountAsync(new JobsFilter(), CancellationToken.None);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetCountAsync_FiltersByStatus()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);
        job.Started(_sampleRunner);
        await _store.UpdateAsync(job, CancellationToken.None);

        await _store.CreateAsync(NewJob(), CancellationToken.None);

        var count = await _store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Scheduled] },
            CancellationToken.None);

        count.Should().Be(1);
    }

    [Fact]
    public async Task GetDetailsAsync_ReturnsJobWithFullAuditTrail()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);
        job.Started(_sampleRunner);
        await _store.UpdateAsync(job, CancellationToken.None);

        var details = await _store.GetDetailsAsync(job.Id, CancellationToken.None);

        details.Id.Should().Be(job.Id);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Created);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Scheduled);
        details.Audit.Should().Contain(a => a.Type == JobAuditRecordType.Started);
    }

    [Fact]
    public async Task GetOneOrDefaultAsync_ReturnsJobMatchingFilter()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);

        var result = await _store.GetOneOrDefaultAsync(new JobQuery
        {
            Filter = new JobsFilter { Statuses = [JobStatus.Scheduled] }
        }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(job.Id);
    }

    [Fact]
    public async Task GetOneOrDefaultAsync_WhenNoMatch_ReturnsNull()
    {
        var result = await _store.GetOneOrDefaultAsync(new JobQuery
        {
            Filter = new JobsFilter { Statuses = [JobStatus.Failed] }
        }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Failed_WithRetries_ReschedulesJobInStore()
    {
        var job = await _store.CreateAsync(NewJob(retryDelays: [30]), CancellationToken.None);
        job.Started(_sampleRunner);
        await _store.UpdateAsync(job, CancellationToken.None);

        var running = (await _store.FindAsync(job.Id, CancellationToken.None))!;
        running.Failed("transient error");
        await _store.UpdateAsync(running, CancellationToken.None);

        var result = await _store.FindAsync(job.Id, CancellationToken.None);
        result!.Status.Should().Be(JobStatus.Scheduled);
        result.RetriesCount.Should().Be(1);
    }

    [Fact]
    public async Task DataPersistedAcrossStoreInstances()
    {
        var job = await _store.CreateAsync(NewJob(), CancellationToken.None);

        await using var freshScope = _serviceProvider.CreateAsyncScope();
        var freshStore = freshScope.ServiceProvider.GetRequiredService<IStore>();
        var result     = await freshStore.FindAsync(job.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(job.Id);
    }
}

[CollectionDefinition("PostgreSQL")]
public class PostgreSqlCollectionDefinition : ICollectionFixture<PostgreSqlFixture> { }
