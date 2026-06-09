using ClubManagementApi.Data;
using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Models.Entities;
using ClubManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubManagementApi.Services.Implementations;

public class BookingService(AppDbContext db) : IBookingService
{
    public async Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(int memberId)
    {
        return await db.Bookings
            .Include(b => b.Member)
            .Include(b => b.Venue)
            .Include(b => b.Slot)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => ToResponse(b))
            .ToListAsync();
    }

    public async Task<IEnumerable<BookingResponse>> GetAllBookingsAsync()
    {
        return await db.Bookings
            .Include(b => b.Member)
            .Include(b => b.Venue)
            .Include(b => b.Slot)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => ToResponse(b))
            .ToListAsync();
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, int memberId)
    {
        // Parse and validate booking date
        if (!DateOnly.TryParse(request.BookingDate, out var bookingDate))
            throw new ArgumentException("Invalid date format. Use YYYY-MM-DD (e.g. 2024-06-15).");

        if (bookingDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Booking date cannot be in the past.");

        // Validate venue exists and is active
        var venue = await db.Venues.FindAsync(request.VenueId)
            ?? throw new KeyNotFoundException($"Venue {request.VenueId} not found.");

        if (!venue.IsActive)
            throw new InvalidOperationException("This venue is currently inactive.");

        // Validate slot belongs to the venue
        var slot = await db.VenueSlots
            .FirstOrDefaultAsync(s => s.Id == request.SlotId && s.VenueId == request.VenueId)
            ?? throw new KeyNotFoundException($"Slot {request.SlotId} not found for venue {request.VenueId}.");

        // Validate booking date matches slot day of week
        var dayOfWeek = bookingDate.DayOfWeek.ToString();
        if (!slot.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"Slot is only available on {slot.DayOfWeek}. The date {bookingDate} is a {dayOfWeek}.");

        // ── Conflict validation ───────────────────────────────────────────────

        // 1. Slot already booked by someone else on this date
        var slotTaken = await db.Bookings.AnyAsync(b =>
            b.SlotId      == request.SlotId &&
            b.BookingDate == bookingDate     &&
            b.Status      == "Confirmed");

        if (slotTaken)
            throw new InvalidOperationException(
                "This slot is already booked for the selected date.");

        // 2. Member already has a booking at overlapping time on the same date
        var memberConflict = await db.Bookings
            .Include(b => b.Slot)
            .AnyAsync(b =>
                b.MemberId    == memberId    &&
                b.BookingDate == bookingDate &&
                b.Status      == "Confirmed" &&
                b.Slot.StartTime < slot.EndTime &&
                b.Slot.EndTime   > slot.StartTime);

        if (memberConflict)
            throw new InvalidOperationException(
                "You already have a booking that overlaps with this time slot.");

        // ─────────────────────────────────────────────────────────────────────

        var booking = new Booking
        {
            MemberId    = memberId,
            VenueId     = request.VenueId,
            SlotId      = request.SlotId,
            BookingDate = bookingDate,
            Status      = "Confirmed"
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        // Reload with navigation properties for response
        await db.Entry(booking).Reference(b => b.Member).LoadAsync();
        await db.Entry(booking).Reference(b => b.Venue).LoadAsync();
        await db.Entry(booking).Reference(b => b.Slot).LoadAsync();

        return ToResponse(booking);
    }

    public async Task CancelAsync(int bookingId, int requestingMemberId, string requestingRole)
    {
        var booking = await db.Bookings.FindAsync(bookingId)
            ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

        if (booking.Status == "Cancelled")
            throw new InvalidOperationException("This booking is already cancelled.");

        // Members can only cancel their own bookings; Admins can cancel any
        if (requestingRole != "Admin" && booking.MemberId != requestingMemberId)
            throw new UnauthorizedAccessException("You can only cancel your own bookings.");

        booking.Status = "Cancelled";
        await db.SaveChangesAsync();
    }

    private static BookingResponse ToResponse(Booking b) => new(
        b.Id,
        b.MemberId,
        b.Member.FullName,
        b.VenueId,
        b.Venue.Name,
        b.SlotId,
        b.Slot.DayOfWeek,
        b.Slot.StartTime.ToString("HH:mm"),
        b.Slot.EndTime.ToString("HH:mm"),
        b.BookingDate.ToString("yyyy-MM-dd"),
        b.Status,
        b.CreatedAt);
}
