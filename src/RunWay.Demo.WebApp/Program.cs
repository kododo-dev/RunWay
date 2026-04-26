using Kododo.RunWay;
using Kododo.RunWay.Dashboard;
using Kododo.RunWay.Demo.WebApp;
using Kododo.RunWay.Demo.WebApp.Database;
using Kododo.RunWay.Demo.WebApp.Jobs;
using Kododo.RunWay.PostgreSQL;
using Kododo.RunWay.Runner;
using Kododo.RunWay.Schedule;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DemoDB");
var services = builder.Services;

services.AddDbContext<DemoDbContext>(x => x.UseNpgsql(connectionString));
services.AddRunWay(x =>
{
    x.AddDashboard()
        .UsePostgreSQL(p => p.GetRequiredService<DemoDbContext>().Database.GetDbConnection())
        .AddRunner(x =>
        {
            x.AddHandlersFromAssembly(typeof(Program).Assembly);
            x.DeleteSucceededAfterTimeSpan = TimeSpan.FromMinutes(10);
        });
    
    x.Options.SchedulerOptions.TransactionalDefault = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await db.Database.EnsureCreatedAsync();
}

var pathBase = builder.Configuration["PathBase"] ?? "/";
if(!pathBase.StartsWith("/")) pathBase = "/" + pathBase;
if(!pathBase.EndsWith("/")) pathBase += "/";

app.MapGet("/", () => Results.Content(HomeView.Render(pathBase), "text/html"));

app.MapPost("/jobs/quick", async (IScheduler runway) =>
{
    await runway.Job(new QuickJob { TaskName = "cache-invalidation" })
        .ScheduleAsync(CancellationToken.None);
    return Results.Ok();
});

app.MapPost("/jobs/report", async (IScheduler runway) =>
{
    await runway.Job(new ReportJob { ReportName = "Monthly Sales Summary", Rows = 12_000 })
        .ScheduleAsync(CancellationToken.None);
    return Results.Ok();
});

app.MapPost("/jobs/sync", async (IScheduler runway) =>
{
    await runway.Job(new DataSyncJob { Source = "orders-db", Destination = "analytics-db", RecordCount = 50_000 })
        .ScheduleAsync(CancellationToken.None);
    return Results.Ok();
});

app.MapPost("/jobs/export", async (IScheduler runway) =>
{
    await runway.Job(new ExportJob { Format = "CSV", TotalRecords = 200_000 })
        .ScheduleAsync(CancellationToken.None);
    return Results.Ok();
});

app.MapPost("/jobs/fail", async (IScheduler runway) =>
{
    await runway.Job(new FailingJob { Reason = "intentional demo error" })
        .ScheduleAsync(CancellationToken.None);
    return Results.Ok();
});

app.UseRunWayDashboard();

await app.InitializeRunWayDatabaseAsync();

await app.SetRecurrenceAsync("5 minutes interval", "*/5 * * * *",
    new RecurringJob { TaskName = "5 minutes interval job" });

app.Run();
