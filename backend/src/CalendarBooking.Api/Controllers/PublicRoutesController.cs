using CalendarBooking.Api.Contracts;
using CalendarBooking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalendarBooking.Api.Controllers;

[ApiController]
[Route("public")]
public sealed class PublicRoutesController(CalendarBookingService booking) : ControllerBase
{
    [HttpGet("event-types")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<EventTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IReadOnlyList<EventTypeDto>> ListEventTypes(CancellationToken cancellationToken) =>
        await booking.ListEventTypesAsync(cancellationToken);

    [HttpGet("event-types/{eventTypeId}/available-slots")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<TimeSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListAvailableSlots(
        [FromRoute] string eventTypeId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(eventTypeId, out var id) || id == Guid.Empty)
        {
            return BadRequest(
                new BadRequestDto
                {
                    Error = "VALIDATION_ERROR",
                    Message = "eventTypeId must be a valid UUID.",
                });
        }

        try
        {
            return Ok(await booking.ListAvailableSlotsAsync(id, cancellationToken));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new NotFoundDto { Message = e.Message });
        }
    }

    [HttpPost("bookings")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ConflictDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequestDto body,
        CancellationToken cancellationToken)
    {
        if (body is null)
        {
            return BadRequest(
                new BadRequestDto
                {
                    Error = "VALIDATION_ERROR",
                    Message = "Request body is required.",
                });
        }

        var result = await booking.CreateBookingAsync(body, cancellationToken);
        return result switch
        {
            BookingCreateOutcome.CreatedResult r => StatusCode(
                StatusCodes.Status201Created,
                r.Booking),
            BookingCreateOutcome.BadRequestResult b => BadRequest(
                new BadRequestDto { Error = b.Error, Message = b.Message }),
            BookingCreateOutcome.NotFoundResult n => NotFound(
                new NotFoundDto { Message = n.Message }),
            BookingCreateOutcome.ConflictResult c => Conflict(
                new ConflictDto { Error = c.Error, Message = c.Message }),
            _ => throw new NotSupportedException(),
        };
    }
}
