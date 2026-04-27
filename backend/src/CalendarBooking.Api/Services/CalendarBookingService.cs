using System.Globalization;
using CalendarBooking.Api.Contracts;
using CalendarBooking.Api.Data;
using CalendarBooking.Api.Data.Entities;
using CalendarBooking.Api.Utilities;
using Microsoft.EntityFrameworkCore;

namespace CalendarBooking.Api.Services;

public sealed class CalendarBookingService(
    AppDbContext db,
    TimeProvider time)
{
    private static readonly int[] ValidDurations = [15, 30, 45, 60];
    public const int GuestNameMax = 500;
    public const int NameMax = 200;
    public const int DescriptionMax = 2000;
    public const int SlotStepMinutes = 15;
    public const int DayStartHour = 9;
    public const int DayEndHour = 18;

    public static bool IsValidDuration(int m) => ValidDurations.Contains(m);

    public async Task<IReadOnlyList<TimeSlotDto>> ListAvailableSlotsAsync(Guid eventTypeId, CancellationToken ct = default)
    {
        var eventType = await db.EventTypes.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventTypeId, ct);
        if (eventType is null)
        {
            throw new KeyNotFoundException("Event type not found.");
        }

        if (!IsValidDuration(eventType.DurationMinutes))
        {
            return [];
        }

        var bookings = await db.Bookings.AsNoTracking()
            .Select(b => new BookingInterval(b.StartAtUtc, b.EndAtUtc))
            .ToListAsync(ct);

        return GetAvailableSlotsCore(eventType.DurationMinutes, bookings);
    }

    public async Task<BookingCreateOutcome> CreateBookingAsync(CreateBookingRequestDto body, CancellationToken ct = default)
    {
        if (body.EventTypeId is null or "" || !Guid.TryParse(body.EventTypeId, out var typeId))
        {
            return BookingCreateOutcome.BadRequest("VALIDATION_ERROR", "eventTypeId must be a valid UUID.");
        }

        if (body.StartAt is null or "" || !DateTimeOffset.TryParse(
                body.StartAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var startAt))
        {
            return BookingCreateOutcome.BadRequest("VALIDATION_ERROR", "startAt is required and must be a valid date-time.");
        }

        if (body.GuestDisplayName is not null && body.GuestDisplayName.Length > GuestNameMax)
        {
            return BookingCreateOutcome.BadRequest("VALIDATION_ERROR", $"guestDisplayName must be at most {GuestNameMax} characters.");
        }

        var nameTrim = body.GuestDisplayName?.Trim();
        if (nameTrim is { Length: 0 })
        {
            return BookingCreateOutcome.BadRequest("VALIDATION_ERROR", "guestDisplayName must not be only whitespace when provided.");
        }

        var eventType = await db.EventTypes.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == typeId, ct);
        if (eventType is null)
        {
            return BookingCreateOutcome.NotFound("Event type not found.");
        }

        if (!IsValidDuration(eventType.DurationMinutes))
        {
            return BookingCreateOutcome.BadRequest("VALIDATION_ERROR", "This event type has an invalid duration.");
        }

        var startUtc = startAt.ToUniversalTime().UtcDateTime;
        var endUtc = startUtc.AddMinutes(eventType.DurationMinutes);
        if (!IsWithinBookingWindowLocal(startAt, GetBookingWindow(), time.LocalTimeZone))
        {
            return BookingCreateOutcome.BadRequest("SLOT_OUTSIDE_WINDOW", "The selected time is outside the 14-day booking window.");
        }

        var allBookings = await db.Bookings.AsNoTracking()
            .Select(b => new BookingInterval(b.StartAtUtc, b.EndAtUtc))
            .ToListAsync(ct);
        if (allBookings.Any(
                b => SlotMath.IntervalsOverlap(
                    b.StartAtUtc,
                    b.EndAtUtc,
                    startUtc,
                    endUtc)))
        {
            return BookingCreateOutcome.Conflict("SLOT_OCCUPIED", "This time slot is already booked.");
        }

        if (!IsProposedSlotInFreeList(eventType.DurationMinutes, allBookings, startUtc, endUtc))
        {
            return BookingCreateOutcome.BadRequest("SLOT_MISMATCH", "startAt does not match a free slot for this event type.");
        }

        var bookingId = Guid.NewGuid();

        var row = new BookingEntity
        {
            Id = bookingId,
            EventTypeId = typeId,
            StartAtUtc = startUtc,
            EndAtUtc = endUtc,
            GuestDisplayName = nameTrim,
        };
        _ = await db.Bookings.AddAsync(row, ct);
        _ = await db.SaveChangesAsync(ct);

        var dto = new BookingDto
        {
            Id = bookingId.ToString(),
            EventTypeId = typeId.ToString(),
            EventTypeName = eventType.Name,
            StartAt = Iso8601Utc.Format(startUtc),
            EndAt = Iso8601Utc.Format(endUtc),
            GuestDisplayName = nameTrim,
        };
        return BookingCreateOutcome.Created(dto);
    }

    public async Task<IReadOnlyList<EventTypeDto>> ListEventTypesAsync(CancellationToken ct = default) =>
        await db.EventTypes.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new EventTypeDto
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Description = x.Description,
                DurationMinutes = x.DurationMinutes,
            })
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BookingDto>> ListUpcomingBookingsAsync(CancellationToken ct = default)
    {
        var now = time.GetUtcNow().UtcDateTime;
        return await db.Bookings.AsNoTracking()
            .Where(b => b.StartAtUtc >= now)
            .Include(b => b.EventType)
            .OrderBy(b => b.StartAtUtc)
            .Select(b => new BookingDto
            {
                Id = b.Id.ToString(),
                EventTypeId = b.EventTypeId.ToString(),
                EventTypeName = b.EventType!.Name,
                StartAt = Iso8601Utc.Format(b.StartAtUtc),
                EndAt = Iso8601Utc.Format(b.EndAtUtc),
                GuestDisplayName = b.GuestDisplayName,
            })
            .ToListAsync(ct);
    }

    public async Task<EventTypeDto> CreateEventTypeAsync(EventTypeCreateDto body, CancellationToken ct = default)
    {
        if (body.Id is null or "" || !Guid.TryParse(body.Id, out var id))
        {
            throw new ArgumentException("id must be a valid UUID.");
        }

        if (string.IsNullOrWhiteSpace(body.Name))
        {
            throw new ArgumentException("name is required.");
        }

        if (string.IsNullOrWhiteSpace(body.Description))
        {
            throw new ArgumentException("description is required.");
        }

        if (body.Name.Length > NameMax)
        {
            throw new ArgumentException($"name must be at most {NameMax} characters.");
        }

        if (body.Description.Length > DescriptionMax)
        {
            throw new ArgumentException($"description must be at most {DescriptionMax} characters.");
        }

        if (body.DurationMinutes is not int dm || !IsValidDuration(dm))
        {
            throw new ArgumentException("durationMinutes must be 15, 30, 45, or 60.");
        }

        if (await db.EventTypes.AnyAsync(e => e.Id == id, ct))
        {
            throw new InvalidOperationException("DUPLICATE_ID");
        }

        var row = new EventTypeEntity
        {
            Id = id,
            Name = body.Name.Trim(),
            Description = body.Description.Trim(),
            DurationMinutes = dm,
        };
        _ = await db.EventTypes.AddAsync(row, ct);
        _ = await db.SaveChangesAsync(ct);

        return new EventTypeDto
        {
            Id = row.Id.ToString(),
            Name = row.Name,
            Description = row.Description,
            DurationMinutes = row.DurationMinutes,
        };
    }

    public async Task<EventTypeDto> UpdateEventTypeAsync(Guid id, EventTypeUpdateDto body, CancellationToken ct = default)
    {
        if (body.Name is null && body.Description is null && body.DurationMinutes is null)
        {
            throw new ArgumentException("At least one field is required for update.");
        }

        if (body.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(body.Name))
            {
                throw new ArgumentException("name must not be empty.");
            }

            if (body.Name.Length > NameMax)
            {
                throw new ArgumentException($"name must be at most {NameMax} characters.");
            }
        }

        if (body.Description is not null)
        {
            if (string.IsNullOrWhiteSpace(body.Description))
            {
                throw new ArgumentException("description must not be empty.");
            }

            if (body.Description.Length > DescriptionMax)
            {
                throw new ArgumentException($"description must be at most {DescriptionMax} characters.");
            }
        }

        if (body.DurationMinutes is { } d && !IsValidDuration(d))
        {
            throw new ArgumentException("durationMinutes must be 15, 30, 45, or 60.");
        }

        var row = await db.EventTypes.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (row is null)
        {
            throw new KeyNotFoundException("Event type not found.");
        }

        if (body.Name is not null)
        {
            row.Name = body.Name.Trim();
        }

        if (body.Description is not null)
        {
            row.Description = body.Description.Trim();
        }

        if (body.DurationMinutes is { } newDur)
        {
            row.DurationMinutes = newDur;
        }

        _ = await db.SaveChangesAsync(ct);

        return new EventTypeDto
        {
            Id = row.Id.ToString(),
            Name = row.Name,
            Description = row.Description,
            DurationMinutes = row.DurationMinutes,
        };
    }

    public async Task DeleteEventTypeAsync(Guid id, CancellationToken ct = default)
    {
        var row = await db.EventTypes.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (row is null)
        {
            throw new KeyNotFoundException("Event type not found.");
        }

        var now = time.GetUtcNow().UtcDateTime;
        var hasFuture = await db.Bookings
            .AnyAsync(b => b.EventTypeId == id && b.StartAtUtc >= now, ct);
        if (hasFuture)
        {
            throw new InvalidOperationException("FUTURE_BOOKINGS_EXIST");
        }

        _ = await db.Bookings.Where(b => b.EventTypeId == id).ExecuteDeleteAsync(ct);
        _ = db.EventTypes.Remove(row);
        _ = await db.SaveChangesAsync(ct);
    }

    private readonly record struct BookingInterval(DateTime StartAtUtc, DateTime EndAtUtc);

    private (DateOnly FirstDay, DateOnly LastDay) GetBookingWindow()
    {
        var tz = time.LocalTimeZone;
        var localNow = TimeZoneInfo.ConvertTime(time.GetUtcNow(), tz);
        var today = DateOnly.FromDateTime(localNow.DateTime);
        return (today, today.AddDays(13));
    }

    private bool IsWithinBookingWindowLocal(
        DateTimeOffset startAt,
        (DateOnly FirstDay, DateOnly LastDay) window,
        TimeZoneInfo tz)
    {
        var local = TimeZoneInfo.ConvertTime(startAt, tz);
        var d = DateOnly.FromDateTime(local.Date);
        return d >= window.FirstDay && d <= window.LastDay;
    }

    private IReadOnlyList<TimeSlotDto> GetAvailableSlotsCore(
        int durationMinutes,
        IReadOnlyList<BookingInterval> bookings)
    {
        var tz = time.LocalTimeZone;
        var slots = new List<TimeSlotDto>();
        var (first, last) = GetBookingWindow();
        for (var day = first; day <= last; day = day.AddDays(1))
        {
            var dayStart = new DateTime(day.Year, day.Month, day.Day, DayStartHour, 0, 0, DateTimeKind.Unspecified);
            var dayEnd = new DateTime(day.Year, day.Month, day.Day, DayEndHour, 0, 0, DateTimeKind.Unspecified);
            for (var t = dayStart; t.AddMinutes(durationMinutes) <= dayEnd; t = t.AddMinutes(SlotStepMinutes))
            {
                var endLocal = t.AddMinutes(durationMinutes);
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(t, tz);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
                if (bookings.Any(b => SlotMath.IntervalsOverlap(b.StartAtUtc, b.EndAtUtc, startUtc, endUtc)))
                {
                    continue;
                }

                slots.Add(new TimeSlotDto
                {
                    StartAt = Iso8601Utc.Format(startUtc),
                    EndAt = Iso8601Utc.Format(endUtc),
                });
            }
        }

        return slots.OrderBy(s => s.StartAt, StringComparer.Ordinal).ToList();
    }

    private bool IsProposedSlotInFreeList(
        int durationMinutes,
        IReadOnlyList<BookingInterval> bookings,
        DateTime startUtc,
        DateTime endUtc)
    {
        var free = GetAvailableSlotsCore(durationMinutes, bookings);
        return free.Any(
            s =>
                ParseUtcToDateTime(s.StartAt) == startUtc && ParseUtcToDateTime(s.EndAt) == endUtc);
    }

    private static DateTime ParseUtcToDateTime(string iso) =>
        DateTimeOffset.Parse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime;
}
