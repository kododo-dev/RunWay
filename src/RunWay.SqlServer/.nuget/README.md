# Kododo.RunWay.SqlServer

SQL Server storage provider for [Kododo.RunWay](https://www.nuget.org/packages/Kododo.RunWay). Without a storage provider, RunWay has no persistence — jobs and runner state are lost on restart.

## Install

```bash
dotnet add package Kododo.RunWay
dotnet add package Kododo.RunWay.SqlServer
```

## Setup

Provide a `DbConnection` from your existing `DbContext` to share the same connection (required for outbox pattern support):

```csharp
builder.Services.AddRunWay(x =>
{
    x.UseSqlServer(p => p.GetRequiredService<AppDbContext>().Database.GetDbConnection());
});
```

Or provide a standalone connection without a shared `DbContext`:

```csharp
builder.Services.AddRunWay(x =>
{
    x.UseSqlServer(_ => new SqlConnection(connectionString));
});
```

## Schema initialization

Call `InitializeRunWayDatabaseAsync` on startup to apply migrations:

```csharp
await app.InitializeRunWayDatabaseAsync();
```

This runs EF Core migrations automatically. Safe to call on every startup.

## Requirements

- SQL Server 2016 or later (also Azure SQL Database)
- The database user needs permission to create tables and schemas on first run

## Links

- Source: https://github.com/kododo-dev/RunWay
