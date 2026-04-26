namespace Kododo.RunWay.Core.Jobs;

public sealed record JobData(string Type, string Data)
{
    public bool Equals(JobData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type && Data == other.Data;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Data);
    }
}