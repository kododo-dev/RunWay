using Kododo.RunWay.Core.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kododo.RunWay.EntityFramework;

public static class RunWayConfigurationExtensions
{
    public static IRunWayConfiguration AddRunWayEntityFramework(this IRunWayConfiguration configuration, Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null)
    {
        configuration.Services.AddDbContext<JobsDbContext>(optionsAction);
        configuration.Services.AddTransient<IStore, Store>();
        return configuration;
    }
}