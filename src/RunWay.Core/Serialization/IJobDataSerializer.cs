using Kododo.RunWay.Core.Jobs;

namespace Kododo.RunWay.Core.Serialization;

public interface IJobDataSerializer
{
    string SerializeType(Type type);
    
    JobData Serialize<T>(T data);
    
    object Deserialize(JobData jobData);
}