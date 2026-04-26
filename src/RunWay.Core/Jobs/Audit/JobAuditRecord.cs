namespace Kododo.RunWay.Core.Jobs.Audit;

public sealed record JobAuditRecord(DateTimeOffset Time, JobAuditRecordType Type, string? Details = null);