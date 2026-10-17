using System.Data.Common;
using Kododo.RunWay.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.SqlServer;

public static class RunWayConfigurationExtensions
{
    public static IRunWayConfiguration UseSqlServer(this IRunWayConfiguration configuration, Func<IServiceProvider, DbConnection> connectionFactory)
    {
        configuration.AddRunWayEntityFramework((serviceProvider, optionsBuilder) =>
            SqlServerDbContextOptionsExtensions.UseSqlServer(optionsBuilder, connectionFactory(serviceProvider),
                options =>
                {
                    options.MigrationsAssembly(typeof(RunWayConfigurationExtensions).Assembly.GetName().Name);
#if NET10_0_OR_GREATER
                    // EF Core 10 sends a list (the job statuses runners poll for) as one parameter per item; the plan
                    // that gives deadlocks runners polling with runners updating jobs. One JSON parameter, as in EF
                    // Core 8 and 9, does not.
                    options.UseParameterizedCollectionMode(ParameterTranslationMode.Parameter);
#endif
                }));

        return configuration;
    }
}
