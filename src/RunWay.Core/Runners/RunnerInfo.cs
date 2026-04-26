namespace Kododo.RunWay.Core.Runners;

public sealed record RunnerInfo(
    RunnerId Id,
    string Name
)
{
    public override string ToString()
    {
        return $"#{Id.Value} {Name}";
    }
}