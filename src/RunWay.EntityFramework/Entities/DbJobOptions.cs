namespace Kododo.RunWay.EntityFramework.Entities;

internal sealed record DbJobOptions(int[] RetryDelays, TimeSpan Timeout);