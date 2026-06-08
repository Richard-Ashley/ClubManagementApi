using ClubManagementApi.Data;
using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Models.Entities;
using ClubManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubManagementApi.Services.Implementations;

public class VenueService(AppDbContext db) : IVenueService
{
    public async Task<IEnumerable<VenueResponse>> GetAllAsync()
    {
        return await db.Venues
            .OrderBy(v => v.Name)
            .Select(v => ToResponse(v))
            .ToListAsync();
    }

    public async Task<VenueResponse> GetByIdAsync(int id)
    {
        var venue = await db.Venues.FindAsync(id)
            ?? throw new KeyNotFoundException($"Venue {id} not found.");

        return ToResponse(venue);
    }

    public async Task<VenueResponse> CreateAsync(CreateVenueRequest request)
    {
        var venue = new Venue
        {
            Name        = request.Name,
            Description = request.Description,
            IsActive    = true
        };

        db.Venues.Add(venue);
        await db.SaveChangesAsync();

        return ToResponse(venue);
    }

    public async Task<VenueResponse> ToggleActiveAsync(int id)
    {
        var venue = await db.Venues.FindAsync(id)
            ?? throw new KeyNotFoundException($"Venue {id} not found.");

        venue.IsActive = !venue.IsActive;
        await db.SaveChangesAsync();

        return ToResponse(venue);
    }

    public async Task<IEnumerable<VenueSlotResponse>> GetSlotsAsync(int venueId)
    {
        var exists = await db.Venues.AnyAsync(v => v.Id == venueId);
        if (!exists)
            throw new KeyNotFoundException($"Venue {venueId} not found.");

        return await db.VenueSlots
            .Where(s => s.VenueId == venueId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => ToSlotResponse(s))
            .ToListAsync();
    }

    public async Task<VenueSlotResponse> AddSlotAsync(int venueId, CreateVenueSlotRequest request)
    {
        var venue = await db.Venues.FindAsync(venueId)
            ?? throw new KeyNotFoundException($"Venue {venueId} not found.");

        if (!TimeOnly.TryParse(request.StartTime, out var start) ||
            !TimeOnly.TryParse(request.EndTime, out var end))
            throw new ArgumentException("Invalid time format. Use HH:mm (e.g. 08:00).");

        if (end <= start)
            throw new ArgumentException("End time must be after start time.");

        var slot = new VenueSlot
        {
            VenueId   = venueId,
            DayOfWeek = request.DayOfWeek,
            StartTime = start,
            EndTime   = end
        };

        db.VenueSlots.Add(slot);
        await db.SaveChangesAsync();

        return ToSlotResponse(slot);
    }

    private static VenueResponse ToResponse(Venue v) =>
        new(v.Id, v.Name, v.Description, v.IsActive);

    private static VenueSlotResponse ToSlotResponse(VenueSlot s) =>
        new(s.Id, s.VenueId, s.DayOfWeek, s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"));
}
