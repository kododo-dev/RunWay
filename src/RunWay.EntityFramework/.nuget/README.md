# Kododo.RunWay.EntityFramework

Entity Framework Core storage foundation for [Kododo.RunWay](https://www.nuget.org/packages/Kododo.RunWay) — the background job queue for .NET.

## When to reference this package

You need this package **only if you are building a custom EF Core storage provider** for RunWay. It contains the shared `DbContext`, entity mappings and `IStore` implementation on top of `Microsoft.EntityFrameworkCore.Relational`.

Application developers should install one of the ready-made providers instead:

- [Kododo.RunWay.PostgreSQL](https://www.nuget.org/packages/Kododo.RunWay.PostgreSQL)
- [Kododo.RunWay.SqlServer](https://www.nuget.org/packages/Kododo.RunWay.SqlServer)

## Install

```bash
dotnet add package Kododo.RunWay.EntityFramework
```

## Building a provider

```csharp
public static IRunWayConfiguration UseMyDatabase(
    this IRunWayConfiguration configuration,
    Func<IServiceProvider, DbConnection> connectionFactory)
{
    return configuration.AddRunWayEntityFramework((serviceProvider, optionsBuilder) =>
        optionsBuilder.UseMyProvider(
            connectionFactory(serviceProvider),
            options => options.MigrationsAssembly(typeof(RunWayConfigurationExtensions).Assembly.GetName().Name)));
}
```

Ship an EF Core migration set built against your provider in the same assembly.

## Links

- Source: https://github.com/kododo-dev/RunWay
