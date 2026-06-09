namespace ClubManagementApi.Models.DTOs;

public record CreateBookingRequest(int VenueId, int SlotId, string BookingDate);

public record BookingResponse(
    int Id,
    int MemberId,
    string MemberName,
    int VenueId,
    string VenueName,
    int SlotId,
    string DayOfWeek,
    string StartTime,
    string EndTime,
    string BookingDate,
    string Status,
    DateTime CreatedAt);
