using System.Globalization;

namespace CalendarBooking.Api.Utilities;

public static class Iso8601Utc
{
    public static string Format(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
