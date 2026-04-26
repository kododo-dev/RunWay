using Kododo.RunWay.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kododo.RunWay.PostgreSQL;

public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
{
    public JobsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JobsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=runway;Username=postgres;Password=postgres",
            options => options.MigrationsAssembly(typeof(RunWayConfigurationExtensions).Assembly.GetName().Name));
        return new JobsDbContext(optionsBuilder.Options);
    }
}

