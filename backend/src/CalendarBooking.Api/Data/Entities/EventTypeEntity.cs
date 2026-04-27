namespace CalendarBooking.Api.Data.Entities;

public sealed class EventTypeEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    /// <summary>15, 30, 45, or 60.</summary>
    public int DurationMinutes { get; set; }

    public ICollection<BookingEntity> Bookings { get; } = new List<BookingEntity>();
}
