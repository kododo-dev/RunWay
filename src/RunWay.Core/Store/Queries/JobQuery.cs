namespace Kododo.RunWay.Core.Store.Queries;

public record JobQuery
{
    public JobsFilter? Filter { get; set; }
    
    public IReadOnlyCollection<JobsSorting>? Sorting { get; set; }
}