namespace CalendarBooking.Tests;

/// <summary>UTC clock fixed to <see cref="Utc"/>. Local time zone is UTC for deterministic slot windows.</summary>
public sealed class FixedTimeProvider : TimeProvider
{
    public FixedTimeProvider(DateTimeOffset utc) => Utc = utc;

    public DateTimeOffset Utc { get; }

    public override DateTimeOffset GetUtcNow() => Utc;

    public override TimeZoneInfo LocalTimeZone { get; } = TimeZoneInfo.Utc;
}
