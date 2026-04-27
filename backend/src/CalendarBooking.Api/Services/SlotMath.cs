namespace CalendarBooking.Api.Services;

public static class SlotMath
{
    /// <summary>Half-open intervals [aStart,aEnd) and [bStart,bEnd).</summary>
    public static bool IntervalsOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd) =>
        aStart < bEnd && bStart < aEnd;
}
