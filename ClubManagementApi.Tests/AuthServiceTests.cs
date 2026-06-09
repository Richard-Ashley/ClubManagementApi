using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Services.Implementations;
using Xunit;

namespace ClubManagementApi.Tests;

public class AuthServiceTests
{
    private AuthService CreateService(string? dbName = null)
    {
        var db     = TestHelpers.CreateDbContext(dbName);
        var config = TestHelpers.CreateJwtConfig();
        return new AuthService(db, config);
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_ReturnsAuthResponse()
    {
        var service  = CreateService();
        var request  = new RegisterRequest("Richard Ashley", "richard@test.com", "Password@123");

        var response = await service.RegisterAsync(request);

        Assert.NotNull(response);
        Assert.Equal("richard@test.com", response.Email);
        Assert.Equal("Richard Ashley",   response.FullName);
        Assert.Equal("Member",           response.Role);
        Assert.False(string.IsNullOrEmpty(response.Token));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsArgumentException()
    {
        var dbName  = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var request = new RegisterRequest("Richard Ashley", "richard@test.com", "Password@123");

        await service.RegisterAsync(request);

        // Second registration with same email should fail
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(request));
    }

    [Fact]
    public async Task Register_AssignsDefaultMemberRole()
    {
        var service  = CreateService();
        var request  = new RegisterRequest("John Doe", "john@test.com", "Password@123");

        var response = await service.RegisterAsync(request);

        Assert.Equal("Member", response.Role);
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        var dbName  = Guid.NewGuid().ToString();
        var service = CreateService(dbName);

        await service.RegisterAsync(new RegisterRequest("Richard Ashley", "richard@test.com", "Password@123"));
        var response = await service.LoginAsync(new LoginRequest("richard@test.com", "Password@123"));

        Assert.NotNull(response);
        Assert.Equal("richard@test.com", response.Email);
        Assert.False(string.IsNullOrEmpty(response.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsKeyNotFoundException()
    {
        var dbName  = Guid.NewGuid().ToString();
        var service = CreateService(dbName);

        await service.RegisterAsync(new RegisterRequest("Richard Ashley", "richard@test.com", "Password@123"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.LoginAsync(new LoginRequest("richard@test.com", "WrongPassword")));
    }

    [Fact]
    public async Task Login_NonExistentEmail_ThrowsKeyNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.LoginAsync(new LoginRequest("ghost@test.com", "Password@123")));
    }

    [Fact]
    public async Task Login_ReturnsJwtToken_ContainingExpectedClaims()
    {
        var dbName  = Guid.NewGuid().ToString();
        var service = CreateService(dbName);

        await service.RegisterAsync(new RegisterRequest("Richard Ashley", "richard@test.com", "Password@123"));
        var response = await service.LoginAsync(new LoginRequest("richard@test.com", "Password@123"));

        // Decode JWT and verify it contains the email claim
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token   = handler.ReadJwtToken(response.Token);

        Assert.Contains(token.Claims, c => c.Value == "richard@test.com");
    }
}
