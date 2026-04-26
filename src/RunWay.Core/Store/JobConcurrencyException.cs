namespace Kododo.RunWay.Core.Store;

public sealed class JobConcurrencyException(string jobId, Exception innerException)
    : Exception($"Job '{jobId}' was modified by another process.", innerException)
{
    public string JobId { get; } = jobId;
}
