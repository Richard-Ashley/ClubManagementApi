using ClubManagementApi.Models.DTOs;

namespace ClubManagementApi.Services.Interfaces;

public interface IVenueService
{
    Task<IEnumerable<VenueResponse>> GetAllAsync();
    Task<VenueResponse> GetByIdAsync(int id);
    Task<VenueResponse> CreateAsync(CreateVenueRequest request);
    Task<VenueResponse> ToggleActiveAsync(int id);
    Task<IEnumerable<VenueSlotResponse>> GetSlotsAsync(int venueId);
    Task<VenueSlotResponse> AddSlotAsync(int venueId, CreateVenueSlotRequest request);
}
