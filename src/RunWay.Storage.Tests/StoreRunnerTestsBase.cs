using FluentAssertions;
using Kododo.RunWay.Core.Runners;
using Xunit;

namespace Kododo.RunWay.Storage.Tests;

public abstract class StoreRunnerTestsBase<TFixture> : StoreTestBase<TFixture>
    where TFixture : class, IStorageTestFixture
{
    protected StoreRunnerTestsBase(TFixture fixture) : base(fixture) { }

    [Fact]
    public async Task RegisterRunnerAsync_ReturnsNonEmptyRunnerId()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        runnerId.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterRunnerAsync_RunnerAppearsInGetAll()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        runners.Should().ContainSingle(r => r.Id == runnerId);
    }

    [Fact]
    public async Task RegisterRunnerAsync_PersistsName()
    {
        await Store.RegisterRunnerAsync("my-runner", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        runners.Should().ContainSingle(r => r.Name == "my-runner");
    }

    [Fact]
    public async Task GetAllRunnersAsync_WhenNoRunners_ReturnsEmpty()
    {
        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);
        runners.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRunnersAsync_ReturnsAllRegisteredRunners()
    {
        await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await Store.RegisterRunnerAsync("runner-2", ["JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        runners.Should().HaveCount(2);
        runners.Select(r => r.Name).Should().BeEquivalentTo("runner-1", "runner-2");
    }

    [Fact]
    public async Task GetAllRunnersAsync_RecentlyRegisteredRunner_IsOnline()
    {
        await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        runners.Single().Status.Should().Be(RunnerStatus.Online);
    }

    [Fact]
    public async Task KeepAliveAsync_DoesNotThrow()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var act = async () => await Store.KeepAliveAsync(runnerId, DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveRunnerAsync_RunnerNoLongerInGetAll()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await Store.RemoveRunnerAsync(runnerId, CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);
        runners.Should().NotContain(r => r.Id == runnerId);
    }

    [Fact]
    public async Task RemoveRunnerAsync_OtherRunnersNotAffected()
    {
        var runnerId1 = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        var runnerId2 = await Store.RegisterRunnerAsync("runner-2", ["JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        await Store.RemoveRunnerAsync(runnerId1, CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);
        runners.Should().ContainSingle(r => r.Id == runnerId2);
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_ReturnsCorrectName()
    {
        var runnerId = await Store.RegisterRunnerAsync("details-runner", ["JobA", "JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await Store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        details.Name.Should().Be("details-runner");
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_ReturnsRegisteredJobTypes()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA", "JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await Store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        details.JobTypes.Should().BeEquivalentTo("JobA", "JobB");
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_InitiallyHasNoJobs()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await Store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        details.Jobs.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeAsync_WhenCalledTwice_IsIdempotent()
    {
        var act = async () => await Store.InitializeAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
