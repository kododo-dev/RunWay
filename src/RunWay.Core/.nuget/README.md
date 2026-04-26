# Kododo.RunWay.Core

Core abstractions and models for [Kododo.RunWay](https://www.nuget.org/packages/Kododo.RunWay) — the background job queue for .NET.

## When to reference this package

You need this package **only if you are building an extension for RunWay**, such as:

- a custom storage backend (`IStore`)
- a custom job handler

Application developers should install `Kododo.RunWay` instead — it already pulls in this package transitively.

## Install

```bash
dotnet add package Kododo.RunWay.Core
```

## Key abstractions

### `IJobHandler<T>`

Implement this interface to define the logic for processing a job:

```csharp
public class SendEmailJobHandler : IJobHandler<SendEmailJob>
{
    public async Task HandleAsync(SendEmailJob data, CancellationToken stoppingToken)
    {
        // process the job
    }
}
```

Register handlers via `AddHandlersFromAssembly` or `AddHandler<THandler, TJob>` inside `AddRunner(...)`.

### `IStore`

Implement this interface to provide a custom storage backend:

```csharp
public interface IStore : IJobStore, INoTransactionalJobStore, IRunnerStore, IRecurrenceStore
{
    Task InitializeAsync(CancellationToken stoppingToken);
    bool IsConcurrencyException(Exception ex);
    Task<ITransaction> BeginTransactionAsync(CancellationToken stoppingToken);
}
```

## Related packages

| Package | Purpose |
|---|---|
| [Kododo.RunWay](https://www.nuget.org/packages/Kododo.RunWay) | Main package — DI registration, `AddRunWay` |
| [Kododo.RunWay.Runner](https://www.nuget.org/packages/Kododo.RunWay.Runner) | Background worker |
| [Kododo.RunWay.Dashboard](https://www.nuget.org/packages/Kododo.RunWay.Dashboard) | Embedded web dashboard |
| [Kododo.RunWay.PostgreSQL](https://www.nuget.org/packages/Kododo.RunWay.PostgreSQL) | PostgreSQL storage provider |

## Links

- Source: https://github.com/kododo-dev/RunWay
