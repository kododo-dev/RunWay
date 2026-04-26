using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.EntityFramework.Entities;

[Table("runner_job_types")]
[PrimaryKey(nameof(RunnerId), nameof(JobType))]
[Index(nameof(JobType))]
internal sealed class DbRunnerJobType
{
    [Column("id")]
    public long RunnerId { get; set; }
    
    [Column("job_type")]
    public string JobType { get; set; } = null!;
    
    [ForeignKey(nameof(RunnerId))]
    public DbRunner? Runner { get; set; }
}