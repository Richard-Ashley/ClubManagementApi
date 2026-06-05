using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClubManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Register a new member account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var response = await authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }
}
