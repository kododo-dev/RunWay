using Microsoft.Extensions.DependencyInjection;

namespace Kododo.RunWay;

public interface IRunWayConfiguration
{
    IServiceCollection Services { get; }
    RunWayOptions Options { get; }
}

internal class RunWayConfiguration(IServiceCollection services) : IRunWayConfiguration
{
    public IServiceCollection Services { get; } = services;
    
    public RunWayOptions Options { get; } = new();
}