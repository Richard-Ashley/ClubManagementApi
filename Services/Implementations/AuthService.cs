using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClubManagementApi.Data;
using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Models.Entities;
using ClubManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ClubManagementApi.Services.Implementations;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await db.Members.AnyAsync(m => m.Email == request.Email);
        if (exists)
            throw new ArgumentException("A member with this email already exists.");

        var member = new Member
        {
            FullName     = request.FullName,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = "Member"
        };

        db.Members.Add(member);
        await db.SaveChangesAsync();

        return new AuthResponse(GenerateToken(member), member.FullName, member.Email, member.Role);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.Email == request.Email)
            ?? throw new KeyNotFoundException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, member.PasswordHash))
            throw new KeyNotFoundException("Invalid email or password.");

        return new AuthResponse(GenerateToken(member), member.FullName, member.Email, member.Role);
    }

    private string GenerateToken(Member member)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(
            double.Parse(config["Jwt:ExpiresInMinutes"] ?? "60"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new Claim(ClaimTypes.Email,          member.Email),
            new Claim(ClaimTypes.Name,           member.FullName),
            new Claim(ClaimTypes.Role,           member.Role)
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
