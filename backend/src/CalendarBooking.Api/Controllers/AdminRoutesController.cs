using CalendarBooking.Api.Contracts;
using CalendarBooking.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalendarBooking.Api.Controllers;

[ApiController]
[Route("admin")]
public sealed class AdminRoutesController(CalendarBookingService booking) : ControllerBase
{
    [HttpGet("bookings")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IReadOnlyList<BookingDto>> ListUpcomingBookings(
        CancellationToken cancellationToken) =>
        await booking.ListUpcomingBookingsAsync(cancellationToken);

    [HttpGet("event-types")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<EventTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IReadOnlyList<EventTypeDto>> ListEventTypesAdmin(
        CancellationToken cancellationToken) =>
        await booking.ListEventTypesAsync(cancellationToken);

    [HttpPost("event-types")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(EventTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEventType(
        [FromBody] EventTypeCreateDto body,
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

        try
        {
            var r = await booking.CreateEventTypeAsync(body, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, r);
        }
        catch (ArgumentException e)
        {
            return BadRequest(
                new BadRequestDto
                {
                    Error = "VALIDATION_ERROR",
                    Message = e.Message,
                });
        }
        catch (InvalidOperationException)
        {
            return BadRequest(
                new BadRequestDto
                {
                    Error = "VALIDATION_ERROR",
                    Message = "An event type with this id already exists.",
                });
        }
    }

    [HttpPatch("event-types/{eventTypeId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(EventTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEventType(
        [FromRoute] string eventTypeId,
        [FromBody] EventTypeUpdateDto body,
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

        if (body is null)
        {
            return BadRequest(
                new BadRequestDto
                {
                    Error = "VALIDATION_ERROR",
                    Message = "Request body is required.",
                });
        }

        try
        {
            return Ok(await booking.UpdateEventTypeAsync(id, body, cancellationToken));
        }
        catch (ArgumentException e)
        {
            return BadRequest(
                new BadRequestDto
                {
                    Error = "VALIDATION_ERROR",
                    Message = e.Message,
                });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new NotFoundDto { Message = e.Message });
        }
    }

    [HttpDelete("event-types/{eventTypeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DeleteEventTypeConflictDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(InternalErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEventType(
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
            await booking.DeleteEventTypeAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new NotFoundDto { Message = e.Message });
        }
        catch (InvalidOperationException)
        {
            return Conflict(
                new DeleteEventTypeConflictDto
                {
                    Error = "FUTURE_BOOKINGS_EXIST",
                    Message = "There are future bookings for this event type.",
                });
        }
    }
}
