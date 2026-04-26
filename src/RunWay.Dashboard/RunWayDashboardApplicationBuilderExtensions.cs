using Kododo.Reiho.AspNetCore.API;
using Kododo.Reiho.AspNetCore.SPA;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Kododo.RunWay.Dashboard;

public static class RunWayDashboardApplicationBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteGroupBuilder UseRunWayDashboard(string path = "/scheduler")
        {
            var runwayEndpoints = endpoints.MapGroup(path);

            runwayEndpoints.MapApi();
            runwayEndpoints.MapEmbeddedSpa(typeof(RunWayDashboardApplicationBuilderExtensions).Assembly);

            return runwayEndpoints;
        }

        private void MapApi()
        {
            var api = endpoints.MapGroup("/api");
            api.MapRequests(typeof(RunWayDashboardApplicationBuilderExtensions).Assembly);
        }
    }
}