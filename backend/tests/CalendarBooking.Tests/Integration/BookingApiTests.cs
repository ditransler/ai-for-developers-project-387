using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CalendarBooking.Api.Contracts;
using Xunit;

namespace CalendarBooking.Tests.Integration;

public class BookingApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public BookingApiTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateBooking_TwiceOnSameSlot_Returns_409_SlotOccupied()
    {
        await _factory.ResetDatabaseAsync();
        var typeId = Guid.NewGuid();
        _ = await _client.PostAsJsonAsync(
            "/admin/event-types",
            new EventTypeCreateDto
            {
                Id = typeId.ToString(),
                Name = "Demo",
                Description = "A demo",
                DurationMinutes = 15,
            });

        var startAt = "2026-04-15T09:00:00.000Z";
        var first = await _client.PostAsJsonAsync(
            "/public/bookings",
            new CreateBookingRequestDto
            {
                EventTypeId = typeId.ToString(),
                StartAt = startAt,
            });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync(
            "/public/bookings",
            new CreateBookingRequestDto
            {
                EventTypeId = typeId.ToString(),
                StartAt = startAt,
            });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var conflict = await second.Content.ReadFromJsonAsync<ConflictDto>(Json);
        Assert.NotNull(conflict);
        Assert.Equal("SLOT_OCCUPIED", conflict.Error);
    }

    [Fact]
    public async Task CreateBooking_MismatchingSlot_Returns_400_SlotMismatch()
    {
        await _factory.ResetDatabaseAsync();
        var typeId = Guid.NewGuid();
        _ = await _client.PostAsJsonAsync(
            "/admin/event-types",
            new EventTypeCreateDto
            {
                Id = typeId.ToString(),
                Name = "Demo",
                Description = "A demo",
                DurationMinutes = 15,
            });

        const string badStart = "2026-04-15T09:07:00.000Z";
        var res = await _client.PostAsJsonAsync(
            "/public/bookings",
            new CreateBookingRequestDto
            {
                EventTypeId = typeId.ToString(),
                StartAt = badStart,
            });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<BadRequestDto>(Json);
        Assert.NotNull(body);
        Assert.Equal("SLOT_MISMATCH", body.Error);
    }

    [Fact]
    public async Task GetAvailableSlots_UnknownType_Returns_404()
    {
        await _factory.ResetDatabaseAsync();
        var id = Guid.NewGuid();
        var res = await _client.GetAsync($"/public/event-types/{id}/available-slots");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task DeleteEventType_WithFutureBooking_Returns_409()
    {
        await _factory.ResetDatabaseAsync();
        var typeId = Guid.NewGuid();
        _ = await _client.PostAsJsonAsync(
            "/admin/event-types",
            new EventTypeCreateDto
            {
                Id = typeId.ToString(),
                Name = "Demo",
                Description = "A demo",
                DurationMinutes = 15,
            });
        _ = await _client.PostAsJsonAsync(
            "/public/bookings",
            new CreateBookingRequestDto
            {
                EventTypeId = typeId.ToString(),
                StartAt = "2026-04-15T10:00:00.000Z",
            });

        var res = await _client.DeleteAsync($"/admin/event-types/{typeId}");
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<DeleteEventTypeConflictDto>(Json);
        Assert.NotNull(body);
        Assert.Equal("FUTURE_BOOKINGS_EXIST", body.Error);
    }

    [Fact]
    public async Task EventTypes_InvalidPathUuid_Returns_400()
    {
        await _factory.ResetDatabaseAsync();
        using var msg = new HttpRequestMessage(
            HttpMethod.Patch,
            "/admin/event-types/not-a-uuid")
        {
            Content = JsonContent.Create(new EventTypeUpdateDto { Name = "x" }),
        };
        var res = await _client.SendAsync(msg);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
