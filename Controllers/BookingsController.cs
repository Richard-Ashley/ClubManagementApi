using System.Security.Claims;
using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    private int CurrentMemberId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role)!;

    /// <summary>Get my bookings.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBookings()
    {
        var bookings = await bookingService.GetMyBookingsAsync(CurrentMemberId);
        return Ok(bookings);
    }

    /// <summary>Get all bookings. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    /// <summary>Create a new booking.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        var booking = await bookingService.CreateAsync(request, CurrentMemberId);
        return CreatedAtAction(nameof(GetMyBookings), new { id = booking.Id }, booking);
    }

    /// <summary>Cancel a booking. Members can cancel their own; Admins can cancel any.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        await bookingService.CancelAsync(id, CurrentMemberId, CurrentRole);
        return NoContent();
    }
}
