using Kododo.RunWay.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kododo.RunWay.SqlServer;

public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
{
    public JobsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JobsDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=runway;User Id=sa;Password=Your_password123;TrustServerCertificate=True",
            options => options.MigrationsAssembly(typeof(RunWayConfigurationExtensions).Assembly.GetName().Name));
        return new JobsDbContext(optionsBuilder.Options);
    }
}
