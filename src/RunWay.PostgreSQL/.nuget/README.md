# Kododo.RunWay.PostgreSQL

PostgreSQL storage provider for [Kododo.RunWay](https://www.nuget.org/packages/Kododo.RunWay). Without a storage provider, RunWay has no persistence — jobs and runner state are lost on restart.

## Install

```bash
dotnet add package Kododo.RunWay
dotnet add package Kododo.RunWay.PostgreSQL
```

## Setup

Provide a `DbConnection` from your existing `DbContext` to share the same connection (required for outbox pattern support):

```csharp
builder.Services.AddRunWay(x =>
{
    x.UsePostgreSQL(p => p.GetRequiredService<AppDbContext>().Database.GetDbConnection());
});
```

Or provide a standalone connection without a shared `DbContext`:

```csharp
builder.Services.AddRunWay(x =>
{
    x.UsePostgreSQL(_ => new NpgsqlConnection(connectionString));
});
```

## Schema initialization

Call `InitializeRunWayDatabaseAsync` on startup to apply migrations:

```csharp
await app.InitializeRunWayDatabaseAsync();
```

This runs EF Core migrations automatically. Safe to call on every startup.

## Requirements

- PostgreSQL 12 or later
- The database user needs `CREATE` privileges on first run

## Links

- Source: https://github.com/kododo-dev/RunWay
