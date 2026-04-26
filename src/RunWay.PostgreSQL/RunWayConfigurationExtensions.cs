using System.Data.Common;
using Kododo.RunWay.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.PostgreSQL;

public static class RunWayConfigurationExtensions
{
    public static IRunWayConfiguration UsePostgreSQL(this IRunWayConfiguration configuration, Func<IServiceProvider, DbConnection> connectionFactory)
    {
        configuration.AddRunWayEntityFramework((serviceProvider, optionsBuilder) =>
            NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql(optionsBuilder, connectionFactory(serviceProvider),
                options => options.MigrationsAssembly(typeof(RunWayConfigurationExtensions).Assembly.GetName().Name)));
        
        return configuration;
    }
}