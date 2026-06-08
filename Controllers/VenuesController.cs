using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class VenuesController(IVenueService venueService) : ControllerBase
{
    /// <summary>Get all venues.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var venues = await venueService.GetAllAsync();
        return Ok(venues);
    }

    /// <summary>Get a venue by id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var venue = await venueService.GetByIdAsync(id);
        return Ok(venue);
    }

    /// <summary>Create a new venue. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateVenueRequest request)
    {
        var venue = await venueService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = venue.Id }, venue);
    }

    /// <summary>Toggle a venue active/inactive. Admin only.</summary>
    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var venue = await venueService.ToggleActiveAsync(id);
        return Ok(venue);
    }

    /// <summary>Get all slots for a venue.</summary>
    [HttpGet("{id}/slots")]
    [ProducesResponseType(typeof(IEnumerable<VenueSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlots(int id)
    {
        var slots = await venueService.GetSlotsAsync(id);
        return Ok(slots);
    }

    /// <summary>Add a slot to a venue. Admin only.</summary>
    [HttpPost("{id}/slots")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VenueSlotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSlot(int id, CreateVenueSlotRequest request)
    {
        var slot = await venueService.AddSlotAsync(id, request);
        return CreatedAtAction(nameof(GetSlots), new { id }, slot);
    }
}
