using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Core.Recurrences;

public class Recurrence(RecurrenceId id, int version, string rule, JobData data, JobOptions options, Job? currentJob)
{
    public RecurrenceId Id { get; } = id;
    
    public int Version { get; } = version;

    public string Rule { get; private set; } = rule;

    public JobData Data { get; private set; } = data;

    public  JobOptions Options { get; private set; } = options;
    
    public Job? CurrentJob { get; private set; } = currentJob;
    
    public static Recurrence CreateNew(RecurrenceId id, string rule, JobData data, JobOptions options)
    {
        return new Recurrence(id, 1, rule, data, options, null);
    }

    public void Update(string rule, JobData data, JobOptions options, Job currentJob)
    {
        this.Rule = rule;
        this.Data = data;
        this.Options = options;
        this.CurrentJob = currentJob;
    }

    public void AssignJob(Job job)
    {
        if (this.CurrentJob?.Status.IsCompleted() == false)
        {
            throw new InvalidOperationException("Cannot assign a new job to a recurrence that has an active job.");
        }
        
        this.CurrentJob = job;
    }
    
    private bool Equals(Recurrence other)
    {
        return Rule == other.Rule && Data.Equals(other.Data) && Options.Equals(other.Options);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Recurrence)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Rule, Data, Options);
    }
}