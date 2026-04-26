using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard;

public static class RunWayConfigurationExtensions
{
    public static IRunWayConfiguration AddDashboard(this IRunWayConfiguration configuration)
    {
        configuration.Services.AddRequestHandlers(typeof(RunWayConfigurationExtensions).Assembly);
        return configuration;
    }
}