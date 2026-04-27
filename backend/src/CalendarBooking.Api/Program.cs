using CalendarBooking.Api.Contracts;
using CalendarBooking.Api.Data;
using CalendarBooking.Api.Infrastructure;
using CalendarBooking.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);
builder.Services.AddProblemDetails();
builder.Services.AddRouting(o => o.LowercaseUrls = false);
builder.Services.AddCors(
    p => p.AddDefaultPolicy(
        s => s.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = ctx =>
{
    var first = ctx.ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage)
        .FirstOrDefault(m => !string.IsNullOrEmpty(m));
    var message = !string.IsNullOrEmpty(first)
        ? first
        : "The request is invalid.";
    return new BadRequestObjectResult(
        new BadRequestDto
        {
            Error = "VALIDATION_ERROR",
            Message = message,
        });
});
builder.Services.AddSingleton(TimeProvider.System);
// Single open connection: required for shared in-memory SQLite so schema is visible to all consumers.
var sqlite = new SqliteConnection("Data Source=bookings;Mode=Memory;Cache=Shared");
sqlite.Open();
builder.Services.AddSingleton(sqlite);
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(sqlite));
builder.Services.AddScoped<CalendarBookingService>();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

_ = app.UseExceptionHandler();
_ = app.UseCors();
_ = app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
}

await app.RunAsync();

public partial class Program
{
}
