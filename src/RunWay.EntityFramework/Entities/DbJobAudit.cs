using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kododo.RunWay.Core.Jobs.Audit;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.EntityFramework.Entities;

[Table("jobs_audit")]
[Index(nameof(JobId), nameof(Time))]
internal sealed class DbJobAudit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("job_id")]
    public long JobId { get; set; }
    
    [Column("time")]
    public DateTime Time { get; set; }
    
    [Column("type")]
    public JobAuditRecordType Type { get; set; }
    
    [Column("details")]
    public string? Details { get; set; }
}