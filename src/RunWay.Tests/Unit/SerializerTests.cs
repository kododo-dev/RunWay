using Kododo.RunWay.Core.Serialization;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

public class SerializerTests
{
    private readonly SystemJsonJobDataSerializer _serializer = new();

    private record SamplePayload(string Name, int Value);

    [Fact]
    public void Serialize_ReturnsJobDataWithCorrectType()
    {
        var result = _serializer.Serialize(new SamplePayload("test", 42));
        Assert.Equal(typeof(SamplePayload).FullName, result.Type);
    }

    [Fact]
    public void Serialize_ReturnsJobDataWithSerializedJson()
    {
        var result = _serializer.Serialize(new SamplePayload("test", 42));
        Assert.Contains("test", result.Data);
        Assert.Contains("42", result.Data);
    }

    [Fact]
    public void Deserialize_ReturnsCorrectObject()
    {
        var jobData = _serializer.Serialize(new SamplePayload("hello", 99));
        var result   = _serializer.Deserialize(jobData);

        Assert.IsType<SamplePayload>(result);
        Assert.Equal("hello", ((SamplePayload)result).Name);
        Assert.Equal(99, ((SamplePayload)result).Value);
    }

    [Fact]
    public void Deserialize_WhenTypeNotFound_Throws()
    {
        var jobData = new Core.Jobs.JobData("NonExistent.Type.That.DoesNotExist", "{}");
        var act = () => _serializer.Deserialize(jobData);
        Assert.ThrowsAny<InvalidOperationException>(act);
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_PreservesAllProperties()
    {
        var original = new SamplePayload("round-trip", 123);
        var jobData  = _serializer.Serialize(original);
        var result   = (SamplePayload)_serializer.Deserialize(jobData);

        Assert.Equal(original, result);
    }

    [Fact]
    public void SerializeType_ReturnsFullName()
    {
        var result = _serializer.SerializeType(typeof(SamplePayload));
        Assert.Equal(typeof(SamplePayload).FullName, result);
    }
}
