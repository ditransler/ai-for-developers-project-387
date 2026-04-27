namespace CalendarBooking.Api.Data.Entities;

public sealed class BookingEntity
{
    public Guid Id { get; set; }

    public Guid EventTypeId { get; set; }

    public EventTypeEntity? EventType { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public string? GuestDisplayName { get; set; }
}
