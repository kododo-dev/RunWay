using Kododo.RunWay.EntityFramework.Conversions;
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

        // Two relationships, not one: a job points at the recurrence that scheduled it, and a recurrence at its next
        // job. Configured here rather than with [ForeignKey] on both navigations, which EF Core first tries to pair.
        modelBuilder.Entity<DbJob>().HasOne(x => x.Recurrence).WithMany().HasForeignKey(x => x.ReccurenceId);
        modelBuilder.Entity<DbRecurrence>().HasOne(x => x.Job).WithMany().HasForeignKey(x => x.JobId);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        this.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }
}
