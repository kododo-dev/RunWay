using Microsoft.Extensions.Options;

namespace Kododo.RunWay.Runner;

internal sealed class JobRunnerOptionsValidator : IValidateOptions<JobRunnerOptions>
{
    public ValidateOptionsResult Validate(string? name, JobRunnerOptions options)
    {
        var errors = new List<string>();

        if (options.ThreadsCount <= 0)
            errors.Add($"{nameof(options.ThreadsCount)} must be greater than 0.");
        
        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
