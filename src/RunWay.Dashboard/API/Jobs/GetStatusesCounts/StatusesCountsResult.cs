namespace Kododo.RunWay.Dashboard.API.Jobs.GetStatusesCounts;

internal sealed record StatusesCountsResult(int Pending, int Running, int Succeeded, int Failed, int Retrying);