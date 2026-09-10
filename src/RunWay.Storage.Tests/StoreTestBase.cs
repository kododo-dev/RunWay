using Kododo.RunWay;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kododo.RunWay.Storage.Tests;

public abstract class StoreTestBase<TFixture> : IAsyncLifetime
    where TFixture : class, IStorageTestFixture
{
    protected TFixture Fixture { get; }

    protected ServiceProvider ServiceProvider { get; private set; } = null!;

    protected AsyncServiceScope Scope { get; private set; }

    protected IStore Store { get; private set; } = null!;

    protected StoreTestBase(TFixture fixture) => Fixture = fixture;

    public virtual async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddRunWay(Fixture.ConfigureStorage);

        ServiceProvider = services.BuildServiceProvider();
        Scope = ServiceProvider.CreateAsyncScope();
        Store = Scope.ServiceProvider.GetRequiredService<IStore>();

        await Store.InitializeAsync(CancellationToken.None);
        await Fixture.ResetAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Scope.DisposeAsync();
        await ServiceProvider.DisposeAsync();
    }

    protected IStore CreateStore(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IStore>();

    protected static Job NewJob(
        string type = "TestJob",
        IReadOnlyList<int>? retryDelays = null,
        DateTimeOffset? scheduledAt = null) =>
        Job.CreateNew(
            new JobData(type, "{}"),
            new JobOptions(0, retryDelays?.ToArray() ?? [], TimeSpan.FromMinutes(5)),
            scheduledAt ?? DateTimeOffset.UtcNow);

    protected async Task<RunnerInfo> RegisterRunnerAsync(string name, string[]? jobTypes = null)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var store = CreateStore(scope);
        var runnerId = await store.RegisterRunnerAsync(
            name, jobTypes ?? ["TestJob"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        return new RunnerInfo(runnerId, name);
    }

    protected static JobQuery ScheduledJobsQuery() => new()
    {
        Filter = new JobsFilter { Statuses = [JobStatus.Scheduled], MaxScheduledAt = DateTimeOffset.UtcNow },
        Sorting = [new JobsSorting(JobSortingField.ScheduledAt)]
    };
}
