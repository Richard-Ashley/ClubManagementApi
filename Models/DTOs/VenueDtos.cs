namespace ClubManagementApi.Models.DTOs;

public record VenueResponse(int Id, string Name, string Description, bool IsActive);

public record VenueSlotResponse(int Id, int VenueId, string DayOfWeek, string StartTime, string EndTime);

public record CreateVenueRequest(string Name, string Description);

public record CreateVenueSlotRequest(string DayOfWeek, string StartTime, string EndTime);
