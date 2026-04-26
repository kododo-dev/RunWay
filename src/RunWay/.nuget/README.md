# Kododo.RunWay

Lightweight background job queue for .NET. Schedule and persist jobs with priority, retries, timeout support, and outbox pattern integration.

## Install

```bash
dotnet add package Kododo.RunWay
dotnet add package Kododo.RunWay.Runner        # background worker
dotnet add package Kododo.RunWay.PostgreSQL    # storage provider
dotnet add package Kododo.RunWay.Dashboard     # optional — web UI
```

## Setup

```csharp
builder.Services.AddRunWay(x =>
{
    x.UsePostgreSQL(p => p.GetRequiredService<AppDbContext>().Database.GetDbConnection())
     .AddRunner(opts => opts.AddHandlersFromAssembly(typeof(Program).Assembly))
     .AddDashboard();
});
```

## Define a job

```csharp
public class SendEmailJob
{
    public required string To      { get; set; }
    public required string Subject { get; set; }
}

public class SendEmailJobHandler : IJobHandler<SendEmailJob>
{
    public async Task HandleAsync(SendEmailJob data, CancellationToken stoppingToken)
    {
        // send email...
    }
}
```

## Schedule a job

```csharp
public class OrderService(IScheduler scheduler)
{
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        await scheduler
            .Job(new SendEmailJob { To = order.Email, Subject = "Order confirmed" })
            .ScheduleAsync(ct);
    }
}
```

## Scheduling options

```csharp
await scheduler
    .Job(new SendEmailJob { To = "user@example.com", Subject = "Hello" })
    .WithPriority(10)                          // higher = processed first (default: 0)
    .WithRetryDelaysInSeconds(10, 60, 300)     // retry after 10s, 60s, 5min
    .WithTimeout(TimeSpan.FromMinutes(5))      // fail job if it exceeds this duration
    .ScheduleAsync(ct);

// Schedule for a future time
await scheduler
    .Job(new ReminderJob { Message = "Don't forget!" })
    .ScheduleAsync(DateTimeOffset.UtcNow.AddHours(2), ct);
```

## Recurring jobs

Register recurring jobs on startup using a standard 5-field cron expression. The expression is validated at startup and stored as-is — shorthand like `*/5 * * * *` is preserved.

```csharp
// Before app.Run()
await app.SetRecurrenceAsync("hourly-report", "0 * * * *", new GenerateReportJob());

// With options
await app.SetRecurrenceAsync("cleanup", "0 2 * * *", new CleanupJob(), opts =>
    opts.WithTimeout(TimeSpan.FromMinutes(30)));
```

Safe to call on every startup — only updates when the expression or data changes.

## Outbox pattern

Enlist job creation in an existing database transaction for guaranteed atomicity:

```csharp
await using var transaction = await db.Database.BeginTransactionAsync();

db.Orders.Add(new Order { ... });
await db.SaveChangesAsync();

// AsTransactional(false) — reuse the ambient transaction instead of opening a new one
await scheduler
    .Job(new SendEmailJob { To = "user@example.com", Subject = "Order confirmed" })
    .AsTransactional(false)
    .ScheduleAsync(ct);

// Both the order and the job are committed or rolled back together
await transaction.CommitAsync();
```

For this to work, RunWay must share the same database connection as your `DbContext`:

```csharp
x.UsePostgreSQL(p => p.GetRequiredService<AppDbContext>().Database.GetDbConnection())
```

## Links

- Source: https://github.com/kododo-dev/RunWay
