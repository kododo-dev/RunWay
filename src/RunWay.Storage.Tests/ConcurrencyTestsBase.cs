using FluentAssertions;
using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Runners;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Core.Store.Queries;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kododo.RunWay.Storage.Tests;

public abstract class ConcurrencyTestsBase<TFixture> : StoreTestBase<TFixture>
    where TFixture : class, IStorageTestFixture
{
    protected ConcurrencyTestsBase(TFixture fixture) : base(fixture) { }

    private static async Task RunUntilEmptyAsync(
        IStore store,
        RunnerInfo runnerInfo,
        int emptyPollsLimit = 5,
        int delayMs = 100)
    {
        var emptyPolls = 0;
        while (emptyPolls < emptyPollsLimit)
        {
            var job = await store.GetOneOrDefaultAsync(ScheduledJobsQuery(), CancellationToken.None);

            if (job == null)
            {
                emptyPolls++;
                await Task.Delay(delayMs, CancellationToken.None);
                continue;
            }

            emptyPolls = 0;
            try
            {
                job.Started(runnerInfo);
                job = await store.UpdateAsync(job, CancellationToken.None);

                job.Succeeded();
                await store.UpdateAsync(job, CancellationToken.None);
            }
            catch (Exception ex) when (store.IsConcurrencyException(ex)) { }
        }
    }

    [Fact]
    public async Task MultipleRunners_AllJobsProcessedExactlyOnce()
    {
        const int jobCount = 20;
        const int runnerCount = 4;

        for (var i = 0; i < jobCount; i++)
            await Store.CreateAsync(NewJob(), CancellationToken.None);

        var runnerTasks = Enumerable.Range(0, runnerCount).Select(async i =>
        {
            var runnerInfo = await RegisterRunnerAsync($"runner-{i}");
            await using var scope = ServiceProvider.CreateAsyncScope();
            await RunUntilEmptyAsync(CreateStore(scope), runnerInfo);
        }).ToArray();

        await Task.WhenAll(runnerTasks);

        var succeeded = await Store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Succeeded] }, CancellationToken.None);
        var remaining = await Store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Scheduled, JobStatus.Running] }, CancellationToken.None);

        succeeded.Should().Be(jobCount, "each job should be processed exactly once");
        remaining.Should().Be(0, "no job should remain unprocessed");
    }

    [Fact]
    public async Task TwoRunners_RaceForSameJob_OnlyOneSucceeds()
    {
        var runnerInfo1 = await RegisterRunnerAsync("racer-1");
        var runnerInfo2 = await RegisterRunnerAsync("racer-2");

        await Store.CreateAsync(NewJob(), CancellationToken.None);

        using var barrier = new Barrier(2);

        async Task<bool> RaceAsync(RunnerInfo runnerInfo)
        {
            await using var scope = ServiceProvider.CreateAsyncScope();
            var store = CreateStore(scope);

            var job = await store.GetOneOrDefaultAsync(ScheduledJobsQuery(), CancellationToken.None);
            if (job == null) return false;

            barrier.SignalAndWait();

            try
            {
                job.Started(runnerInfo);
                job = await store.UpdateAsync(job, CancellationToken.None);
                job.Succeeded();
                await store.UpdateAsync(job, CancellationToken.None);
                return true;
            }
            catch (Exception ex) when (store.IsConcurrencyException(ex))
            {
                return false;
            }
        }

        var results = await Task.WhenAll(RaceAsync(runnerInfo1), RaceAsync(runnerInfo2));

        results.Count(r => r).Should().Be(1, "exactly one runner should win the race");
        results.Count(r => !r).Should().Be(1, "the other runner should get ConcurrencyException");

        var count = await Store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Succeeded] }, CancellationToken.None);
        count.Should().Be(1);
    }

    [Fact]
    public async Task MultipleThreadsPerRunner_WithSemaphore_AllJobsProcessed()
    {
        const int jobCount = 10;
        const int threadCount = 4;

        for (var i = 0; i < jobCount; i++)
            await Store.CreateAsync(NewJob(), CancellationToken.None);

        var runnerInfo = await RegisterRunnerAsync("multi-thread-runner");
        using var semaphore = new SemaphoreSlim(1, 1);

        var threadTasks = Enumerable.Range(0, threadCount).Select(async _ =>
        {
            await using var scope = ServiceProvider.CreateAsyncScope();
            var store = CreateStore(scope);

            var emptyPolls = 0;
            while (emptyPolls < 5)
            {
                Job? job;
                await semaphore.WaitAsync(CancellationToken.None);
                try { job = await store.GetOneOrDefaultAsync(ScheduledJobsQuery(), CancellationToken.None); }
                finally { semaphore.Release(); }

                if (job == null)
                {
                    emptyPolls++;
                    await Task.Delay(100, CancellationToken.None);
                    continue;
                }

                emptyPolls = 0;
                try
                {
                    job.Started(runnerInfo);
                    job = await store.UpdateAsync(job, CancellationToken.None);
                    job.Succeeded();
                    await store.UpdateAsync(job, CancellationToken.None);
                }
                catch (Exception ex) when (store.IsConcurrencyException(ex)) { }
            }
        }).ToArray();

        await Task.WhenAll(threadTasks);

        var succeeded = await Store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Succeeded] }, CancellationToken.None);

        succeeded.Should().Be(jobCount, "the semaphore should prevent double-fetching of a job");
    }

    [Fact]
    public async Task HighLoad_ManyRunners_ManyJobs_AllJobsSucceed()
    {
        const int jobCount = 50;
        const int runnerCount = 8;

        for (var i = 0; i < jobCount; i++)
            await Store.CreateAsync(NewJob(), CancellationToken.None);

        var runnerTasks = Enumerable.Range(0, runnerCount).Select(async i =>
        {
            var runnerInfo = await RegisterRunnerAsync($"stress-runner-{i}");
            await using var scope = ServiceProvider.CreateAsyncScope();
            await RunUntilEmptyAsync(CreateStore(scope), runnerInfo);
        }).ToArray();

        await Task.WhenAll(runnerTasks);

        var succeededCount = await Store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Succeeded] }, CancellationToken.None);

        succeededCount.Should().Be(jobCount);
    }

    [Fact]
    public async Task TwoRunners_DifferentJobTypes_EachProcessesOnlyItsOwnJobs()
    {
        var runnerAId = await Store.RegisterRunnerAsync("runner-type-a", ["TypeA"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        var runnerBId = await Store.RegisterRunnerAsync("runner-type-b", ["TypeB"], DateTimeOffset.UtcNow.AddMinutes(1), CancellationToken.None);
        var runnerA = new RunnerInfo(runnerAId, "runner-type-a");
        var runnerB = new RunnerInfo(runnerBId, "runner-type-b");

        for (var i = 0; i < 5; i++)
        {
            await Store.CreateAsync(NewJob("TypeA"), CancellationToken.None);
            await Store.CreateAsync(NewJob("TypeB"), CancellationToken.None);
        }

        async Task ProcessAllAsync(RunnerInfo runnerInfo, RunnerId filterId)
        {
            await using var scope = ServiceProvider.CreateAsyncScope();
            var store = CreateStore(scope);

            while (true)
            {
                var job = await store.GetOneOrDefaultAsync(new JobQuery
                {
                    Filter = new JobsFilter
                    {
                        Statuses = [JobStatus.Scheduled],
                        MaxScheduledAt = DateTimeOffset.UtcNow,
                        SuitableForRunnerId = filterId
                    },
                    Sorting = [new JobsSorting(JobSortingField.ScheduledAt)]
                }, CancellationToken.None);

                if (job == null) break;

                job.Started(runnerInfo);
                job = await store.UpdateAsync(job, CancellationToken.None);
                job.Succeeded();
                await store.UpdateAsync(job, CancellationToken.None);
            }
        }

        await Task.WhenAll(
            ProcessAllAsync(runnerA, runnerAId),
            ProcessAllAsync(runnerB, runnerBId));

        var succeededCount = await Store.GetCountAsync(
            new JobsFilter { Statuses = [JobStatus.Succeeded] }, CancellationToken.None);

        succeededCount.Should().Be(10, "each runner should process its 5 jobs");
    }
}
