using FluentAssertions;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.PostgreSQL.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Kododo.RunWay.PostgreSQL.Tests;

[Collection("PostgreSQL")]
public class StoreRunnerTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private ServiceProvider            _serviceProvider = null!;
    private AsyncServiceScope          _scope;
    private IStore                     _store = null!;

    public StoreRunnerTests(PostgreSqlFixture fixture) => _fixture = fixture;

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
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task RegisterRunnerAsync_ReturnsNonEmptyRunnerId()
    {
        var runnerId = await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        runnerId.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterRunnerAsync_RunnerAppearsInGetAll()
    {
        var runnerId = await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await _store.GetAllRunnersAsync(CancellationToken.None);

        runners.Should().ContainSingle(r => r.Id == runnerId);
    }

    [Fact]
    public async Task RegisterRunnerAsync_PersistsName()
    {
        await _store.RegisterRunnerAsync("my-runner", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await _store.GetAllRunnersAsync(CancellationToken.None);

        runners.Should().ContainSingle(r => r.Name == "my-runner");
    }

    [Fact]
    public async Task GetAllRunnersAsync_WhenNoRunners_ReturnsEmpty()
    {
        var runners = await _store.GetAllRunnersAsync(CancellationToken.None);
        runners.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRunnersAsync_ReturnsAllRegisteredRunners()
    {
        await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await _store.RegisterRunnerAsync("runner-2", ["JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await _store.GetAllRunnersAsync(CancellationToken.None);

        runners.Should().HaveCount(2);
        runners.Select(r => r.Name).Should().BeEquivalentTo("runner-1", "runner-2");
    }

    [Fact]
    public async Task GetAllRunnersAsync_RecentlyRegisteredRunner_IsOnline()
    {
        await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await _store.GetAllRunnersAsync(CancellationToken.None);

        runners.Single().Status.Should().Be(RunnerStatus.Online);
    }

    [Fact]
    public async Task KeepAliveAsync_DoesNotThrow()
    {
        var runnerId = await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var act = async () => await _store.KeepAliveAsync(runnerId, DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveRunnerAsync_RunnerNoLongerInGetAll()
    {
        var runnerId = await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await _store.RemoveRunnerAsync(runnerId, CancellationToken.None);

        var runners = await _store.GetAllRunnersAsync(CancellationToken.None);
        runners.Should().NotContain(r => r.Id == runnerId);
    }

    [Fact]
    public async Task RemoveRunnerAsync_OtherRunnersNotAffected()
    {
        var runnerId1 = await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        var runnerId2 = await _store.RegisterRunnerAsync("runner-2", ["JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        await _store.RemoveRunnerAsync(runnerId1, CancellationToken.None);

        var runners = await _store.GetAllRunnersAsync(CancellationToken.None);
        runners.Should().ContainSingle(r => r.Id == runnerId2);
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_ReturnsCorrectName()
    {
        var runnerId = await _store.RegisterRunnerAsync("details-runner", ["JobA", "JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await _store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        details.Name.Should().Be("details-runner");
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_ReturnsRegisteredJobTypes()
    {
        var runnerId = await _store.RegisterRunnerAsync("runner-1", ["JobA", "JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await _store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        details.JobTypes.Should().BeEquivalentTo("JobA", "JobB");
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_InitiallyHasNoJobs()
    {
        var runnerId = await _store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await _store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        details.Jobs.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeAsync_WhenCalledTwice_IsIdempotent()
    {
        var act = async () => await _store.InitializeAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
