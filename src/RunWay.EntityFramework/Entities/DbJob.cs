using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Kododo.RunWay.Core.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.EntityFramework.Entities;

[Table("jobs")]
[Index(nameof(Status))]
[Index(nameof(ScheduledAt))]
[Index(nameof(Status), nameof(ScheduledAt))]
internal sealed class DbJob
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [ConcurrencyCheck]
    [Column("version")]
    public int Version { get; set; }
    
    [Column("modified_at")]
    public DateTime ModifiedAt { get; set; }
    
    [Column("status")]
    public JobStatus Status { get; set; }
    
    [Column("runner_id")]
    public long? RunnerId { get; set; }
    
    [Column("type")]
    public string Type { get; set; } = null!;
    
    [Column("data")]
    public string Data { get; set; } = null!;
    
    [Column("priority")]
    public int Priority { get; set; }

    [Column("options")]
    public string SerializedOptions { get; set; } = null!;
    
    [Column("scheduled_at")]
    public DateTime ScheduledAt { get; set; }
    
    [Column("retries_count")]
    public int RetriesCount { get; set; }
    
    [Column("recurrence_id")]
    public long? ReccurenceId { get; set; }

    [NotMapped]
    public DbJobOptions Options
    {
        get => JsonSerializer.Deserialize<DbJobOptions>(this.SerializedOptions)!;
        set => this.SerializedOptions = JsonSerializer.Serialize(value);
    }
    
    [ForeignKey(nameof(RunnerId))]
    public DbRunner? Runner { get; set; }
    
    [ForeignKey(nameof(ReccurenceId))]
    public DbRecurrence? Recurrence { get; set; }
}