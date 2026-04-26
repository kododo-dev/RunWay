using Kododo.RunWay.Core.Jobs.Audit;

namespace Kododo.RunWay.Dashboard.API.Jobs.GetJobDetails;

internal sealed record AuditItem(DateTime Time, JobAuditRecordType Type, string? Details);