using Kododo.RunWay.Core.Jobs.Audit;
using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Runners;

namespace Kododo.RunWay.Core.Jobs;

public sealed class Job(JobId id, int version, JobData data, JobOptions options, RecurrenceId? recurrenceId, JobStatus status, RunnerId? runnerId, DateTimeOffset modifiedAt, DateTimeOffset scheduledAt, int retriesCount)
{
    private readonly List<JobAuditRecord> _pendingEvents = [];
    
    public static Job CreateNew(JobData data, JobOptions options, DateTimeOffset scheduledAt, RecurrenceId? recurrenceId = null)
    {
        var newJob = new Job(new JobId(""), 1, data, options, recurrenceId, JobStatus.Scheduled, null, DateTimeOffset.UtcNow, scheduledAt, 0);
        newJob._pendingEvents.Add(new JobAuditRecord(DateTimeOffset.UtcNow, JobAuditRecordType.Created));
        newJob._pendingEvents.Add(new JobAuditRecord(DateTimeOffset.UtcNow, JobAuditRecordType.Scheduled, scheduledAt.UtcDateTime.ToString("O")));
        return newJob;
    }

    public static Job CreateNew(Recurrence recurrence, DateTimeOffset scheduledAt)
    {
        return CreateNew(recurrence.Data, recurrence.Options, scheduledAt, recurrence.Id);
    }
    
    public JobId Id { get; } = id;
    
    public int Version { get; } = version;

    public JobData Data { get; } = data;

    public  JobOptions Options { get; } = options;

    public JobStatus Status { get; private set; } = status;
    
    public int RetriesCount { get; private set; } = retriesCount;

    public DateTimeOffset ModifiedAt { get; } = modifiedAt;

    public DateTimeOffset ScheduledAt { get; private set; } = scheduledAt;

    public RunnerId? RunnerId { get; private set; } = runnerId;

    public RecurrenceId? RecurrenceId { get; private set; } = recurrenceId;
    
    public IEnumerable<JobAuditRecord> PendingEvents => _pendingEvents;

    public void Started(RunnerInfo runnerInfo)
    {
        if(this.RunnerId != null || this.Status != JobStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled jobs that are not being processed by any runner can be started.");
        
        this.Status = JobStatus.Running;
        this.RunnerId = runnerInfo.Id;
        this._pendingEvents.Add(new JobAuditRecord(DateTimeOffset.UtcNow, JobAuditRecordType.Started, runnerInfo.ToString()));
    }

    public void Succeeded()
    {
        if(this.Status != JobStatus.Running)
            throw new InvalidOperationException("Only running jobs can be marked as succeeded.");
        
        this.Status = JobStatus.Succeeded;
        this.RunnerId = null;
        this._pendingEvents.Add(new JobAuditRecord(DateTimeOffset.UtcNow, JobAuditRecordType.Succeeded));
    }

    public void Requeue()
    {
        if (this.Status != JobStatus.Failed)
            throw new InvalidOperationException("Only failed jobs can be requeued.");

        this.Status = JobStatus.Scheduled;
        this.ScheduledAt = DateTimeOffset.UtcNow;
        this.RunnerId = null;
        this.RetriesCount++;
        this._pendingEvents.Add(new JobAuditRecord(DateTimeOffset.UtcNow, JobAuditRecordType.Scheduled, this.ScheduledAt.UtcDateTime.ToString("O")));
    }

    public void Failed(string failureReason)
    {
        if(this.Status != JobStatus.Running)
            throw new InvalidOperationException("Only running jobs can be marked as failed.");
        
        this._pendingEvents.Add(new JobAuditRecord(DateTimeOffset.UtcNow, JobAuditRecordType.Failed, failureReason));
        this.RunnerId = null;
        
        if (this.Options.RetryDelaysInSeconds.Count > this.RetriesCount)
        {
            var delayInSeconds = this.Options.RetryDelaysInSeconds[this.RetriesCount];
            this.ScheduledAt = DateTimeOffset.UtcNow.AddSeconds(delayInSeconds);
            this.RetriesCount++;
            this.Status = JobStatus.Scheduled;
            this._pendingEvents.Add(new JobAuditRecord(DateTimeOffset.UtcNow, JobAuditRecordType.Scheduled, this.ScheduledAt.UtcDateTime.ToString("O")));
            return;
        }
        
        this.Status = JobStatus.Failed;
    }
}