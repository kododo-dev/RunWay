using Kododo.RunWay.Core.Jobs;
using Kododo.RunWay.Core.Store.Queries;

namespace Kododo.RunWay.Dashboard.Utils;

internal class Query
{
    private List<JobStatus>? _statuses;
    private int? _minRetriesCount;
    private List<JobsSorting>? _sorting; 

    public Query WhereStatus(params JobStatus[] statuses)
    {
        _statuses ??= [];
        _statuses.AddRange(statuses);

        return this;
    }
    
    public Query WhereMinRetriesCount(int minRetriesCount)
    {
        _minRetriesCount = minRetriesCount;
        return this;
    }
    
    public Query OrderBy(JobSortingField field)
    {
        _sorting ??= [];
        _sorting.Add(new JobsSorting(field));
        
        return this;
    }
    
    public Query OrderByDescending(JobSortingField field)
    {
        _sorting ??= [];
        _sorting.Add(new JobsSorting(field, false));
        
        return this;
    }

    public JobsQuery ToMany(int page)
    {
        return new JobsQuery
        {
            Pagination = new Pagination(page),
            Filter = new JobsFilter
            {
                MinRetriesCount = _minRetriesCount,
                Statuses = _statuses
            },
            Sorting = _sorting
        };
    }
}