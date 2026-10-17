using Kododo.Reiho.AspNetCore.API;
using Kododo.Way.Shell;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Kododo.RunWay.Dashboard;

public static class RunWayDashboardApplicationBuilderExtensions
{
    /// <summary>The dashboard as a Way.Shell module: <c>SPA/dist</c> is built with the wayModule() preset.</summary>
    internal static readonly WayModule Module =
        new("scheduler", "Scheduler", typeof(RunWayDashboardApplicationBuilderExtensions).Assembly, "SPA/dist") { Icon = "schedule" };

    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// Mounts the dashboard and its API at <paramref name="path"/>, in Way.Shell. The API is at <c>{path}/api</c>.
        /// </summary>
        /// <returns>
        /// The group of the dashboard's endpoints, for authorization and other conventions. Outside the Development
        /// environment the dashboard and its API answer <c>403</c> until authorization is required here, anonymous
        /// access is allowed explicitly (<c>AllowAnonymous()</c>) or <c>ShellOptions.AllowAnonymous</c> is set.
        /// </returns>
        /// <example>
        /// <code>
        /// app.UseRunWayDashboard("/scheduler").RequireAuthorization("Admin");
        /// </code>
        /// </example>
        public RouteGroupBuilder UseRunWayDashboard(string path = "/scheduler")
        {
            var runwayEndpoints = endpoints.MapWay(Module, path);
            runwayEndpoints.MapGroup("/api").MapRequests(typeof(RunWayDashboardApplicationBuilderExtensions).Assembly);
            return runwayEndpoints;
        }
    }
}
