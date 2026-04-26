using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Kododo.RunWay.Core.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Kododo.RunWay.EntityFramework.Entities;

[Table("recurrences")]
[Index(nameof(Key), IsUnique = true)]
internal sealed class DbRecurrence
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("key")]
    public string Key { get; set; } = null!;
    
    [ConcurrencyCheck]
    [Column("version")]
    public int Version { get; set; }
    
    [Column("type")]
    public string Type { get; set; } = null!;
    
    [Column("data")]
    public string Data { get; set; } = null!;

    [Column("options")]
    public string SerializedOptions { get; set; } = null!;

    [Column("expression")]
    public string Expression { get; set; } = null!;
    
    [Column("job_id")]
    public long? JobId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("modified_at")]
    public DateTime ModifiedAt { get; set; }
    
    [NotMapped]
    public JobOptions Options
    {
        get => JsonSerializer.Deserialize<JobOptions>(this.SerializedOptions)!;
        set => this.SerializedOptions = JsonSerializer.Serialize(value);
    }
    
    [ForeignKey(nameof(JobId))]
    public DbJob? Job { get; set; }
}