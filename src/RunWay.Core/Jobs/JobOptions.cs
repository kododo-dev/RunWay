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
        return Priority == other.Priority && RetryDelaysInSeconds.Equals(other.RetryDelaysInSeconds) && Timeout.Equals(other.Timeout);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Priority, RetryDelaysInSeconds, Timeout);
    }
}