using System.Text.Json.Serialization;

namespace CalendarBooking.Api.Contracts;

public sealed class BadRequestDto
{
    [JsonPropertyName("error")]
    public required string Error { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

public sealed class NotFoundDto
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

public sealed class ConflictDto
{
    [JsonPropertyName("error")]
    public required string Error { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

public sealed class InternalErrorDto
{
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

public sealed class DeleteEventTypeConflictDto
{
    [JsonPropertyName("error")]
    public required string Error { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

public sealed class BookingDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("eventTypeId")]
    public required string EventTypeId { get; init; }

    [JsonPropertyName("eventTypeName")]
    public required string EventTypeName { get; init; }

    [JsonPropertyName("startAt")]
    public required string StartAt { get; init; }

    [JsonPropertyName("endAt")]
    public required string EndAt { get; init; }

    [JsonPropertyName("guestDisplayName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GuestDisplayName { get; init; }
}

public sealed class TimeSlotDto
{
    [JsonPropertyName("startAt")]
    public required string StartAt { get; init; }

    [JsonPropertyName("endAt")]
    public required string EndAt { get; init; }
}

public sealed class CreateBookingRequestDto
{
    [JsonPropertyName("eventTypeId")]
    public string? EventTypeId { get; init; }

    [JsonPropertyName("startAt")]
    public string? StartAt { get; init; }

    [JsonPropertyName("guestDisplayName")]
    public string? GuestDisplayName { get; init; }
}

public sealed class EventTypeDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; init; }
}

public sealed class EventTypeCreateDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("durationMinutes")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? DurationMinutes { get; init; }
}

public sealed class EventTypeUpdateDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("durationMinutes")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? DurationMinutes { get; init; }
}
