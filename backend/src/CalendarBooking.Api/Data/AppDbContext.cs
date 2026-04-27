using CalendarBooking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalendarBooking.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<EventTypeEntity> EventTypes => Set<EventTypeEntity>();

    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventTypeEntity>(b =>
        {
            b.ToTable("event_types");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(500);
            b.Property(x => x.Description).HasMaxLength(4000);
        });

        modelBuilder.Entity<BookingEntity>(b =>
        {
            b.ToTable("bookings");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.StartAtUtc, x.EndAtUtc });
            b.Property(x => x.GuestDisplayName).HasMaxLength(500);
            b.HasOne(x => x.EventType)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
