namespace Kododo.RunWay.Core.Jobs;

public sealed record JobOptions(
    int Priority,
    IReadOnlyList<int> RetryDelaysInSeconds,
    TimeSpan Timeout)
{
    public static JobOptions Default { get; } = new(
        0,
        [0, 10, 60, 600],
        TimeSpan.FromMinutes(10));
    
    public bool Equals(JobOptions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        // The delays are compared by value: options read from the store are a different list instance.
        return Priority == other.Priority && RetryDelaysInSeconds.SequenceEqual(other.RetryDelaysInSeconds) && Timeout.Equals(other.Timeout);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Priority);
        hash.Add(Timeout);
        foreach (var delay in RetryDelaysInSeconds) hash.Add(delay);
        return hash.ToHashCode();
    }
}