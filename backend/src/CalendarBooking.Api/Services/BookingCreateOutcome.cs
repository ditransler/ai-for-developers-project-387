using CalendarBooking.Api.Contracts;

namespace CalendarBooking.Api.Services;

public abstract record BookingCreateOutcome
{
    public sealed record CreatedResult(BookingDto Booking) : BookingCreateOutcome;

    public sealed record BadRequestResult(string Error, string Message) : BookingCreateOutcome;

    public sealed record NotFoundResult(string Message) : BookingCreateOutcome;

    public sealed record ConflictResult(string Error, string Message) : BookingCreateOutcome;

    public static BookingCreateOutcome BadRequest(string error, string message) => new BadRequestResult(error, message);

    public static BookingCreateOutcome NotFound(string message) => new NotFoundResult(message);

    public static BookingCreateOutcome Conflict(string error, string message) => new ConflictResult(error, message);

    public static BookingCreateOutcome Created(BookingDto dto) => new CreatedResult(dto);
}
