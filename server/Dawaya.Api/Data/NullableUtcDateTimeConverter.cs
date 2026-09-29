using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dawaya.Api.Data;

public sealed class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    v => v == null ? v : v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime(),
    v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));
