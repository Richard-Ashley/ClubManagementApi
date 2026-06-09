using ClubManagementApi.Models.DTOs;

namespace ClubManagementApi.Services.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(int memberId);
    Task<IEnumerable<BookingResponse>> GetAllBookingsAsync();
    Task<BookingResponse> CreateAsync(CreateBookingRequest request, int memberId);
    Task CancelAsync(int bookingId, int requestingMemberId, string requestingRole);
}
