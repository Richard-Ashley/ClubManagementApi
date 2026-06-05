using ClubManagementApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClubManagementApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<VenueSlot> VenueSlots => Set<VenueSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Member
        modelBuilder.Entity<Member>(e =>
        {
            e.HasIndex(m => m.Email).IsUnique();
            e.Property(m => m.Role).HasDefaultValue("Member");
        });

        // Booking — prevent double-booking the same slot on the same date
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasIndex(b => new { b.SlotId, b.BookingDate, b.Status })
             .HasFilter("\"Status\" = 'Confirmed'")
             .IsUnique();
        });

        // Seed data
        modelBuilder.Entity<Venue>().HasData(
            new Venue { Id = 1, Name = "Main Hall",     Description = "Large multi-purpose hall", IsActive = true },
            new Venue { Id = 2, Name = "Tennis Court",  Description = "Outdoor clay court",        IsActive = true },
            new Venue { Id = 3, Name = "Swimming Pool", Description = "Olympic-size pool",          IsActive = true }
        );

        modelBuilder.Entity<VenueSlot>().HasData(
            new VenueSlot { Id = 1, VenueId = 1, DayOfWeek = "Monday",    StartTime = new TimeOnly(8, 0),  EndTime = new TimeOnly(10, 0) },
            new VenueSlot { Id = 2, VenueId = 1, DayOfWeek = "Monday",    StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0) },
            new VenueSlot { Id = 3, VenueId = 2, DayOfWeek = "Tuesday",   StartTime = new TimeOnly(7, 0),  EndTime = new TimeOnly(9, 0)  },
            new VenueSlot { Id = 4, VenueId = 2, DayOfWeek = "Thursday",  StartTime = new TimeOnly(7, 0),  EndTime = new TimeOnly(9, 0)  },
            new VenueSlot { Id = 5, VenueId = 3, DayOfWeek = "Wednesday", StartTime = new TimeOnly(6, 0),  EndTime = new TimeOnly(8, 0)  },
            new VenueSlot { Id = 6, VenueId = 3, DayOfWeek = "Friday",    StartTime = new TimeOnly(6, 0),  EndTime = new TimeOnly(8, 0)  }
        );

        // Seed admin user (password: Admin@123)
        modelBuilder.Entity<Member>().HasData(
            new Member
            {
                Id = 1,
                FullName = "Admin",
                Email = "admin@clubapi.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
