using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kododo.RunWay.EntityFramework.Entities;

[Table("runners")]
internal sealed class DbRunner
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; } = null!;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("last_seen_at")]
    public DateTime LastSeenAt { get; set; }
    
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }
    
    public ICollection<DbRunnerJobType> JobTypes { get; set; } = null!;
    
    public ICollection<DbJob> AssignedJobs { get; set; } = null!;
}