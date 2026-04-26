using Kododo.RunWay.Core.Serialization;

namespace Kododo.RunWay;

public class RunWayOptions
{
    public IJobDataSerializer JobDataSerializer { get; set; } = new SystemJsonJobDataSerializer();
    public SchedulerOptions SchedulerOptions { get; } = new SchedulerOptions();
}