using System.Collections.Concurrent;
using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Core.Serialization;

public class SystemJsonJobDataSerializer : IJobDataSerializer
{
    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();


    public string SerializeType(Type type)
    {
        return type.FullName 
               ?? throw new InvalidOperationException("Type must have a full name.");
    }

    public JobData Serialize<T>(T data)
    {
        var type = SerializeType(typeof(T));
        var serialized = System.Text.Json.JsonSerializer.Serialize(data);
        return new JobData(type, serialized);
    }

    public object Deserialize(JobData jobData)
    {
        var type = TypeCache.GetOrAdd(jobData.Type, typeName =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t != null)
            ?? throw new InvalidOperationException($"Type '{typeName}' could not be found.")
        );

        var obj = System.Text.Json.JsonSerializer.Deserialize(jobData.Data, type);
        return obj ?? throw new InvalidOperationException("Deserialization resulted in null.");
    }
}