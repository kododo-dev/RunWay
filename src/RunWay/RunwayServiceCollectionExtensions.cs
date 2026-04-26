using Kododo.RunWay.Core.Recurrences;
using Kododo.RunWay.Core.Store;
using Kododo.RunWay.Schedule;
using Microsoft.Extensions.DependencyInjection;

namespace Kododo.RunWay;

public static class RunWayServiceCollectionExtensions
{
    public static IServiceCollection AddRunWay(this IServiceCollection services, Action<IRunWayConfiguration> configure)
    {
        var configuration = new RunWayConfiguration(services);
        configure(configuration);

        services.AddSingleton(configuration.Options);
        services.AddTransient<IScheduler, Scheduler>();
        services.AddTransient<IRecurrenceCalculator, NCrontabRecurrenceCalculator>();
        services.AddTransient<IJobStore>(x => x.GetRequiredService<IStore>());
        services.AddTransient<IRunnerStore>(x => x.GetRequiredService<IStore>());
        services.AddTransient<IRecurrenceStore>(x => x.GetRequiredService<IStore>());
        
        return services;
    }
}