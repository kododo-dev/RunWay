using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kododo.RunWay.EntityFramework.Conversions;

internal sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
