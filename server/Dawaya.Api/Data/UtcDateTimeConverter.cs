using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dawaya.Api.Data;

// All dates are stored and returned as UTC (CLAUDE.md architecture rule 8).
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
