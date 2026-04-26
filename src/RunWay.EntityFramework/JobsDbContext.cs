using Kododo.RunWay.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.EntityFramework;

public class JobsDbContext(DbContextOptions<JobsDbContext> options) : DbContext(options)
{
    internal DbSet<DbJob> Jobs { get; set; } = null!;
    
    internal DbSet<DbJobAudit> JobsAudit { get; set; } = null!;

    internal DbSet<DbRunner> Runners { get; set; } = null!;
    
    internal DbSet<DbRunnerJobType> RunnerJobTypes { get; set; } = null!;
    
    internal DbSet<DbRecurrence> Recurrences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("runway");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        this.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }
}