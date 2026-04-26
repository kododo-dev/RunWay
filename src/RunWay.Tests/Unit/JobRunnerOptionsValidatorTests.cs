using FluentAssertions;
using Kododo.RunWay.Runner;
using Xunit;

namespace Kododo.RunWay.Tests.Unit;

public class JobRunnerOptionsValidatorTests
{
    private readonly JobRunnerOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenThreadsCountIsZero_Fails()
    {
        var options = new JobRunnerOptions { ThreadsCount = 0 };
        var result  = _validator.Validate(null, options);
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenThreadsCountIsNegative_Fails()
    {
        var options = new JobRunnerOptions { ThreadsCount = -1 };
        var result  = _validator.Validate(null, options);
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenThreadsCountIsPositive_Succeeds()
    {
        var options = new JobRunnerOptions { ThreadsCount = 4 };
        var result  = _validator.Validate(null, options);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenThreadsCountIsOne_Succeeds()
    {
        var options = new JobRunnerOptions { ThreadsCount = 1 };
        var result  = _validator.Validate(null, options);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFailed_FailureMessageMentionsThreadsCount()
    {
        var options = new JobRunnerOptions { ThreadsCount = 0 };
        var result  = _validator.Validate(null, options);
        result.FailureMessage.Should().Contain(nameof(JobRunnerOptions.ThreadsCount));
    }
}
