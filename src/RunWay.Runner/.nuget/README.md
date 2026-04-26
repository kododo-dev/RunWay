# Kododo.RunWay.Runner

Background job runner for [Kododo.RunWay](https://www.nuget.org/packages/Kododo.RunWay). Polls the job queue and executes registered handlers with configurable concurrency, retries, and heartbeat.

## Install

```bash
dotnet add package Kododo.RunWay
dotnet add package Kododo.RunWay.Runner
dotnet add package Kododo.RunWay.PostgreSQL  # storage provider required
```

## Setup

```csharp
builder.Services.AddRunWay(x =>
{
    x.UsePostgreSQL(p => p.GetRequiredService<AppDbContext>().Database.GetDbConnection())
     .AddRunner(opts =>
     {
         opts.AddHandlersFromAssembly(typeof(Program).Assembly);
     });
});
```

## Registering handlers

```csharp
// From a specific assembly — discovers all IJobHandler<T> implementations automatically
opts.AddHandlersFromAssembly(typeof(Program).Assembly);

// Individually
opts.AddHandler<SendEmailJobHandler, SendEmailJob>();
```

## Configuration

```csharp
opts =>
{
    opts.Name              = Environment.MachineName;          // runner name shown in dashboard (default: machine name)
    opts.ThreadsCount      = 4;                                // parallel processing threads (default: processor count)
    opts.Interval          = TimeSpan.FromSeconds(5);          // polling interval when queue is empty (default: 5s)
    opts.HeartbeatInterval = TimeSpan.FromSeconds(30);         // keep-alive signal interval (default: 30s)
    opts.HeartbeatTimeout  = TimeSpan.FromMinutes(2);          // runner considered offline after this time without heartbeat (default: 2min)
    opts.DeleteSucceededAfterTimeSpan = TimeSpan.FromHours(24); // auto-delete succeeded jobs (default: disabled)
}
```

## Links

- Source: https://github.com/kododo-dev/RunWay
