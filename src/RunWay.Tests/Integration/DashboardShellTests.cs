using System.Net;
using System.Text;
using Kododo.RunWay.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kododo.RunWay.Tests.Integration;

/// <summary>The dashboard runs in Way.Shell: the shell's page, the dashboard's module, and the shell's protection.</summary>
public class DashboardShellTests
{
    private static async Task<(WebApplication App, HttpClient Client)> StartAsync(Action<RouteGroupBuilder>? decide)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.Services.AddRunWay(x => x.AddDashboard());
        var app = builder.Build();
        var group = app.UseRunWayDashboard("/jobs");
        decide?.Invoke(group);
        await app.StartAsync();
        return (app, app.GetTestClient());
    }

    [Fact]
    public async Task Page_IsTheShellWithTheDashboardsModule()
    {
        var (app, client) = await StartAsync(g => g.AllowAnonymous());
        await using var _ = app;

        var html = await client.GetStringAsync("/jobs/");

        Assert.Contains("""<base href="/jobs/">""", html);
        Assert.Contains("""<script type="importmap">""", html);
        Assert.Contains("\"moduleUrl\":\"/jobs/_way/way.js?v=", html);
        Assert.Contains("\"cssUrl\":\"/jobs/_way/way.css?v=", html);
        Assert.Contains("\"apiBase\":\"/jobs/api\"", html);
        Assert.Contains("<title>Scheduler</title>", html);
    }

    [Fact]
    public async Task Module_IsServedFromTheDashboardsAssembly()
    {
        var (app, client) = await StartAsync(g => g.AllowAnonymous());
        await using var _ = app;

        var module = await client.GetAsync("/jobs/_way/way.js");

        Assert.Equal(HttpStatusCode.OK, module.StatusCode);
        Assert.Contains("contract:2", (await module.Content.ReadAsStringAsync()).Replace(" ", ""));
    }

    [Theory]
    [InlineData("/jobs/")]
    [InlineData("/jobs/_way/way.js")]
    public async Task WithoutADecisionOnAuthorization_ItIsForbidden_OutsideDevelopment(string url)
    {
        var (app, client) = await StartAsync(decide: null);
        await using var _ = app;

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(url)).StatusCode);
        var api = await client.PostAsync("/jobs/api/GetStatusesCounts", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Forbidden, api.StatusCode);
    }
}
