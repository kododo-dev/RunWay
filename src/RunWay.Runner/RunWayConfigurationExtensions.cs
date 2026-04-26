using Kododo.RunWay.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kododo.RunWay.Runner;

public static class RunWayConfigurationExtensions
{
    public static IRunWayConfiguration AddRunner(this IRunWayConfiguration configuration, Action<JobRunnerOptions>? configureOptions = null)
    {
        var tempOptions = new JobRunnerOptions();
        configureOptions?.Invoke(tempOptions);

        configuration.Services.Configure(configureOptions ?? (_ => { }));
        configuration.Services.AddSingleton<IValidateOptions<JobRunnerOptions>, JobRunnerOptionsValidator>();
        
        foreach (var handlerType in tempOptions.HandlerTypes)
        {
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobHandler<>));

            foreach (var handlerInterface in handlerInterfaces)
            {
                configuration.Services.AddScoped(handlerInterface, handlerType);
            }
        }

        configuration.Services.AddHostedService<JobRunner>();
        return configuration;
    }
}