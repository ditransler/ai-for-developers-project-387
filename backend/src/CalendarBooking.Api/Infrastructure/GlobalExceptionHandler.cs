using System.Net.Mime;
using CalendarBooking.Api.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace CalendarBooking.Api.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        var isDevelopment = string.Equals(
            env?.EnvironmentName,
            "Development",
            StringComparison.Ordinal);
        (int code, object body) t = exception switch
        {
            ArgumentException e => (StatusCodes.Status400BadRequest, new BadRequestDto
            {
                Error = "VALIDATION_ERROR",
                Message = e.Message,
            }),
            KeyNotFoundException e => (StatusCodes.Status404NotFound, new NotFoundDto
            {
                Message = e.Message,
            }),
            InvalidOperationException { Message: "DUPLICATE_ID" } => (
                StatusCodes.Status400BadRequest,
                (object)new BadRequestDto
                {
                    Error = "VALIDATION_ERROR",
                    Message = "An event type with this id already exists.",
                }),
            InvalidOperationException { Message: "FUTURE_BOOKINGS_EXIST" } => (
                StatusCodes.Status409Conflict,
                (object)new DeleteEventTypeConflictDto
                {
                    Error = "FUTURE_BOOKINGS_EXIST",
                    Message = "There are future bookings for this event type.",
                }),
            _ => (StatusCodes.Status500InternalServerError, new InternalErrorDto
            {
                Message = isDevelopment
                    ? exception.ToString()
                    : "An unexpected error occurred.",
            }),
        };

        httpContext.Response.StatusCode = t.code;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        await httpContext.Response.WriteAsJsonAsync(t.body, cancellationToken);
        return true;
    }
}
