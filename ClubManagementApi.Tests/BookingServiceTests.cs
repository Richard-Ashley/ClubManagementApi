using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Models.Entities;
using ClubManagementApi.Services.Implementations;
using Xunit;

namespace ClubManagementApi.Tests;

public class BookingServiceTests
{
    private static readonly DateOnly NextMonday = GetNextWeekday(DayOfWeek.Monday);
    private static readonly DateOnly NextTuesday = GetNextWeekday(DayOfWeek.Tuesday);

    private static DateOnly GetNextWeekday(DayOfWeek target)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        while (date.DayOfWeek != target) date = date.AddDays(1);
        return date;
    }

    /// <summary>Creates a db seeded with one venue, one slot, and one member.</summary>
    private static (BookingService service, int memberId, int venueId, int slotId) CreateSeededService()
    {
        var db = TestHelpers.CreateDbContext();

        var venue = new Venue { Id = 1, Name = "Test Hall", Description = "Test", IsActive = true };
        var slot  = new VenueSlot
        {
            Id        = 1,
            VenueId   = 1,
            DayOfWeek = "Monday",
            StartTime = new TimeOnly(8, 0),
            EndTime   = new TimeOnly(10, 0)
        };
        var member = new Member
        {
            Id           = 1,
            FullName     = "Richard Ashley",
            Email        = "richard@test.com",
            PasswordHash = "hash",
            Role         = "Member"
        };

        db.Venues.Add(venue);
        db.VenueSlots.Add(slot);
        db.Members.Add(member);
        db.SaveChanges();

        return (new BookingService(db), member.Id, venue.Id, slot.Id);
    }

    // ── Create booking ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_ValidRequest_ReturnsBookingResponse()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();

        var response = await service.CreateAsync(
            new CreateBookingRequest(venueId, slotId, NextMonday.ToString("yyyy-MM-dd")),
            memberId);

        Assert.NotNull(response);
        Assert.Equal("Confirmed",      response.Status);
        Assert.Equal("Richard Ashley", response.MemberName);
        Assert.Equal("Test Hall",      response.VenueName);
    }

    [Fact]
    public async Task CreateBooking_SameSlotSameDate_ThrowsInvalidOperationException()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();
        var date = NextMonday.ToString("yyyy-MM-dd");

        await service.CreateAsync(new CreateBookingRequest(venueId, slotId, date), memberId);

        // Second booking on same slot/date should conflict
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBookingRequest(venueId, slotId, date), memberId));
    }

    [Fact]
    public async Task CreateBooking_WrongDayOfWeek_ThrowsArgumentException()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();

        // Slot is Monday — booking on Tuesday should fail
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(
                new CreateBookingRequest(venueId, slotId, NextTuesday.ToString("yyyy-MM-dd")),
                memberId));
    }

    [Fact]
    public async Task CreateBooking_PastDate_ThrowsArgumentException()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateBookingRequest(venueId, slotId, yesterday), memberId));
    }

    [Fact]
    public async Task CreateBooking_InactiveVenue_ThrowsInvalidOperationException()
    {
        var db = TestHelpers.CreateDbContext();

        var venue  = new Venue { Id = 1, Name = "Closed Hall", Description = "Test", IsActive = false };
        var slot   = new VenueSlot { Id = 1, VenueId = 1, DayOfWeek = "Monday", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(10, 0) };
        var member = new Member { Id = 1, FullName = "Test", Email = "t@t.com", PasswordHash = "h", Role = "Member" };

        db.Venues.Add(venue);
        db.VenueSlots.Add(slot);
        db.Members.Add(member);
        db.SaveChanges();

        var service = new BookingService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(
                new CreateBookingRequest(1, 1, NextMonday.ToString("yyyy-MM-dd")), 1));
    }

    [Fact]
    public async Task CreateBooking_InvalidVenue_ThrowsKeyNotFoundException()
    {
        var (service, memberId, _, slotId) = CreateSeededService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(new CreateBookingRequest(999, slotId, NextMonday.ToString("yyyy-MM-dd")), memberId));
    }

    // ── Cancel booking ───────────────────────────────────────────────────────

    [Fact]
    public async Task CancelBooking_Owner_Succeeds()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();
        var booking = await service.CreateAsync(
            new CreateBookingRequest(venueId, slotId, NextMonday.ToString("yyyy-MM-dd")), memberId);

        await service.CancelAsync(booking.Id, memberId, "Member");

        var bookings = await service.GetMyBookingsAsync(memberId);
        Assert.All(bookings, b => Assert.Equal("Cancelled", b.Status));
    }

    [Fact]
    public async Task CancelBooking_NonOwnerMember_ThrowsUnauthorizedAccessException()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();
        var booking = await service.CreateAsync(
            new CreateBookingRequest(venueId, slotId, NextMonday.ToString("yyyy-MM-dd")), memberId);

        var otherMemberId = 999;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.CancelAsync(booking.Id, otherMemberId, "Member"));
    }

    [Fact]
    public async Task CancelBooking_Admin_CanCancelAnyBooking()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();
        var booking = await service.CreateAsync(
            new CreateBookingRequest(venueId, slotId, NextMonday.ToString("yyyy-MM-dd")), memberId);

        // Admin cancelling someone else's booking — should succeed
        await service.CancelAsync(booking.Id, 999, "Admin");

        var bookings = await service.GetMyBookingsAsync(memberId);
        Assert.All(bookings, b => Assert.Equal("Cancelled", b.Status));
    }

    [Fact]
    public async Task CancelBooking_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var (service, memberId, venueId, slotId) = CreateSeededService();
        var booking = await service.CreateAsync(
            new CreateBookingRequest(venueId, slotId, NextMonday.ToString("yyyy-MM-dd")), memberId);

        await service.CancelAsync(booking.Id, memberId, "Member");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CancelAsync(booking.Id, memberId, "Member"));
    }
}
