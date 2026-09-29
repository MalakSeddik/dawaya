using Dawaya.Api.Data;

namespace Dawaya.Tests.Data;

public sealed class UtcDateTimeConverterTests
{
    private readonly UtcDateTimeConverter _converter = new();
    private readonly NullableUtcDateTimeConverter _nullable = new();

    [Fact]
    public void Values_read_from_the_database_are_marked_utc()
    {
        var fromDb = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Unspecified);

        var result = (DateTime)_converter.ConvertFromProvider(fromDb)!;

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(fromDb.Ticks, result.Ticks);
    }

    [Fact]
    public void Utc_values_are_stored_unchanged()
    {
        var utc = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc);

        Assert.Equal(utc, (DateTime)_converter.ConvertToProvider(utc)!);
    }

    [Fact]
    public void Nullable_null_stays_null()
    {
        Assert.Null(_nullable.ConvertFromProvider(null));
        Assert.Null(_nullable.ConvertToProvider(null));
    }
}
