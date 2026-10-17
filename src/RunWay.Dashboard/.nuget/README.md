# Kododo.RunWay.Dashboard

Embedded web dashboard for [Kododo.RunWay](https://www.nuget.org/packages/Kododo.RunWay). Inspect job statuses, audit timelines, and runner health directly from your ASP.NET Core application — no separate deployment required.

## Install

```bash
dotnet add package Kododo.RunWay
dotnet add package Kododo.RunWay.Dashboard
```

## Setup

```csharp
builder.Services.AddRunWay(x =>
{
    x.UsePostgreSQL(...)
     .AddDashboard();   // registers API handlers
});

app.UseRunWayDashboard();  // mounts dashboard at /scheduler
```

## Custom path

```csharp
app.UseRunWayDashboard("/jobs/dashboard");
```

## Authorization

`UseRunWayDashboard()` returns a `RouteGroupBuilder`. Outside the Development environment the dashboard answers `403` until you decide who may use it:

```csharp
app.UseRunWayDashboard()
   .RequireAuthorization(policy => policy.RequireRole("Admin"));

app.UseRunWayDashboard().AllowAnonymous();   // a public demo
```

## Way.Shell

The dashboard runs in [Kododo.Way.Shell](https://www.nuget.org/packages/Kododo.Way.Shell): the layout, theme, language and session handling come from the shell. Configure the title, links and user with `services.AddWayShell(o => ...)`, and add `app.UseWayShell("/admin")` for one panel with every Way in the app.

## Features

- Job list with pagination and filtering by status
- Per-job audit timeline showing status transitions and error details
- Recurring job schedules with cron expressions and job history
- Runner overview with health and heartbeat status

## Links

- Source: https://github.com/kododo-dev/RunWay
