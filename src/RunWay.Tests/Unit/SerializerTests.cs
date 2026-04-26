using FluentAssertions;
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
        result.Type.Should().Be(typeof(SamplePayload).FullName);
    }

    [Fact]
    public void Serialize_ReturnsJobDataWithSerializedJson()
    {
        var result = _serializer.Serialize(new SamplePayload("test", 42));
        result.Data.Should().Contain("test").And.Contain("42");
    }

    [Fact]
    public void Deserialize_ReturnsCorrectObject()
    {
        var jobData = _serializer.Serialize(new SamplePayload("hello", 99));
        var result   = _serializer.Deserialize(jobData);

        result.Should().BeOfType<SamplePayload>();
        ((SamplePayload)result).Name.Should().Be("hello");
        ((SamplePayload)result).Value.Should().Be(99);
    }

    [Fact]
    public void Deserialize_WhenTypeNotFound_Throws()
    {
        var jobData = new Core.Jobs.JobData("NonExistent.Type.That.DoesNotExist", "{}");
        var act = () => _serializer.Deserialize(jobData);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_PreservesAllProperties()
    {
        var original = new SamplePayload("round-trip", 123);
        var jobData  = _serializer.Serialize(original);
        var result   = (SamplePayload)_serializer.Deserialize(jobData);

        result.Should().Be(original);
    }

    [Fact]
    public void SerializeType_ReturnsFullName()
    {
        var result = _serializer.SerializeType(typeof(SamplePayload));
        result.Should().Be(typeof(SamplePayload).FullName);
    }
}
