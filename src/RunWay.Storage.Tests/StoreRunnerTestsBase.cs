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
        Assert.False(string.IsNullOrEmpty(runnerId.Value));
    }

    [Fact]
    public async Task RegisterRunnerAsync_RunnerAppearsInGetAll()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        Assert.Single(runners, r => r.Id == runnerId);
    }

    [Fact]
    public async Task RegisterRunnerAsync_PersistsName()
    {
        await Store.RegisterRunnerAsync("my-runner", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        Assert.Single(runners, r => r.Name == "my-runner");
    }

    [Fact]
    public async Task GetAllRunnersAsync_WhenNoRunners_ReturnsEmpty()
    {
        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);
        Assert.Empty(runners);
    }

    [Fact]
    public async Task GetAllRunnersAsync_ReturnsAllRegisteredRunners()
    {
        await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await Store.RegisterRunnerAsync("runner-2", ["JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        Assert.Equal(2, runners.Count);
        Assert.Equal(new[] { "runner-1", "runner-2" }, runners.Select(r => r.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task GetAllRunnersAsync_RecentlyRegisteredRunner_IsOnline()
    {
        await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);

        Assert.Equal(RunnerStatus.Online, runners.Single().Status);
    }

    [Fact]
    public async Task KeepAliveAsync_DoesNotThrow()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var act = async () => await Store.KeepAliveAsync(runnerId, DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        Assert.Null(await Record.ExceptionAsync(act));
    }

    [Fact]
    public async Task RemoveRunnerAsync_RunnerNoLongerInGetAll()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        await Store.RemoveRunnerAsync(runnerId, CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);
        Assert.DoesNotContain(runners, r => r.Id == runnerId);
    }

    [Fact]
    public async Task RemoveRunnerAsync_OtherRunnersNotAffected()
    {
        var runnerId1 = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        var runnerId2 = await Store.RegisterRunnerAsync("runner-2", ["JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        await Store.RemoveRunnerAsync(runnerId1, CancellationToken.None);

        var runners = await Store.GetAllRunnersAsync(CancellationToken.None);
        Assert.Single(runners, r => r.Id == runnerId2);
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_ReturnsCorrectName()
    {
        var runnerId = await Store.RegisterRunnerAsync("details-runner", ["JobA", "JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await Store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        Assert.Equal("details-runner", details.Name);
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_ReturnsRegisteredJobTypes()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA", "JobB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await Store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        Assert.Equal(new[] { "JobA", "JobB" }, details.JobTypes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task GetRunnerDetailsAsync_InitiallyHasNoJobs()
    {
        var runnerId = await Store.RegisterRunnerAsync("runner-1", ["JobA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);

        var details = await Store.GetRunnerDetailsAsync(runnerId, CancellationToken.None);

        Assert.Empty(details.Jobs);
    }

    [Fact]
    public async Task InitializeAsync_WhenCalledTwice_IsIdempotent()
    {
        var act = async () => await Store.InitializeAsync(CancellationToken.None);
        Assert.Null(await Record.ExceptionAsync(act));
    }
}
