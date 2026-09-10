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
                options => options.MigrationsAssembly(typeof(RunWayConfigurationExtensions).Assembly.GetName().Name)));

        return configuration;
    }
}
