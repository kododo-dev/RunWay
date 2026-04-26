using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Runners;

namespace Kododo.RunWay.Core.Store.Queries;

public sealed record JobsFilter
{
    public IReadOnlyCollection<JobStatus>? Statuses { get; set; }
    
    public int? MinRetriesCount { get; set; }
    
    public DateTimeOffset? MaxScheduledAt { get; set; }
    
    public RunnerId? SuitableForRunnerId { get; set; }
    
    public RunnerId? AssignedRunnerId { get; set; }
    
    public DateTimeOffset? MaxModifiedAt { get; set; }
    
    public RecurrenceId? RecurrenceId { get; set; }
}