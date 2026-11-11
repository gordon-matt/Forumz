using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Forumz.Data;

/// <summary>
/// Providers hand timestamps back as <see cref="DateTimeKind.Unspecified"/>. Stamping them UTC on
/// the way out keeps <c>ToLocalTime()</c> honest.
/// </summary>
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
{
    /// <summary>Shared converter instance.</summary>
    public static UtcDateTimeConverter Instance { get; } = new();
}

/// <summary>Same as <see cref="UtcDateTimeConverter"/> for an optional timestamp.</summary>
public sealed class UtcNullableDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    value => value == null
        ? null
        : value.Value.Kind == DateTimeKind.Local
            ? value.Value.ToUniversalTime()
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
    value => value == null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
{
    /// <summary>Shared converter instance.</summary>
    public static UtcNullableDateTimeConverter Instance { get; } = new();
}
