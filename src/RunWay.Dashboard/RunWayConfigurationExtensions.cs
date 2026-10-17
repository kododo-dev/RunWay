using Kododo.Reiho.AspNetCore.API;
using Kododo.Way.Shell;

namespace Kododo.RunWay.Dashboard;

public static class RunWayConfigurationExtensions
{
    /// <summary>
    /// Adds the dashboard's API handlers and Way.Shell, which the dashboard runs in. Mount it with
    /// <c>app.UseRunWayDashboard(path)</c>; configure the shell (title, links, user) with
    /// <c>services.AddWayShell(o => ...)</c>.
    /// </summary>
    public static IRunWayConfiguration AddDashboard(this IRunWayConfiguration configuration)
    {
        configuration.Services.AddRequestHandlers(typeof(RunWayConfigurationExtensions).Assembly);
        configuration.Services.AddWayShell();
        return configuration;
    }
}
