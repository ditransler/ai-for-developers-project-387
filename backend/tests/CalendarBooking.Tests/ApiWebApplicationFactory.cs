using CalendarBooking.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CalendarBooking.Tests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly DateTimeOffset FixedInstant = new(2026, 4, 15, 10, 0, 0, TimeSpan.Zero);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.UseEnvironment("Testing");
        _ = builder.ConfigureTestServices(
            services =>
            {
                _ = services.RemoveAll<TimeProvider>();
                _ = services.AddSingleton<TimeProvider>(new FixedTimeProvider(FixedInstant));
            });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        _ = await db.Bookings.ExecuteDeleteAsync();
        _ = await db.EventTypes.ExecuteDeleteAsync();
    }
}
