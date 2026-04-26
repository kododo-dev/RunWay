using System.Reflection;
using Kododo.RunWay.Core.Handlers;

namespace Kododo.RunWay.Runner;

public class JobRunnerOptions
{
    public string Name { get; set; } = Environment.MachineName;
    public int ThreadsCount { get; set; } = Environment.ProcessorCount;
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan MaintenanceInterval { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan? DeleteSucceededAfterTimeSpan { get; set; } = null;
    internal List<Type> HandlerTypes { get; } = [];
    
    public JobRunnerOptions AddHandler<THandler, TJob>() where THandler : class, IJobHandler<TJob>
    {
        HandlerTypes.Add(typeof(THandler));
        return this;
    }

    public JobRunnerOptions AddHandlersFromAssembly(Assembly assembly)
    {
        var handlers = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IJobHandler<>)));

        HandlerTypes.AddRange(handlers);
        return this;
    }
}