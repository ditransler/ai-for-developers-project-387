using CalendarBooking.Api.Services;
using Xunit;

namespace CalendarBooking.Tests;

public class SlotMathTests
{
    [Fact]
    public void IntervalsOverlap_TouchingBoundaries_DoNotOverlap()
    {
        var a0 = new DateTime(2026, 4, 15, 9, 0, 0, DateTimeKind.Utc);
        var a1 = a0.AddMinutes(15);
        var b0 = a1;
        var b1 = b0.AddMinutes(15);
        Assert.False(SlotMath.IntervalsOverlap(a0, a1, b0, b1));
    }

    [Fact]
    public void IntervalsOverlap_Interior_Overlap()
    {
        var a0 = new DateTime(2026, 4, 15, 9, 0, 0, DateTimeKind.Utc);
        var a1 = a0.AddHours(1);
        var b0 = a0.AddMinutes(30);
        var b1 = b0.AddHours(1);
        Assert.True(SlotMath.IntervalsOverlap(a0, a1, b0, b1));
    }
}
