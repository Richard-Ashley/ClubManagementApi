using ClubManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ClubManagementApi.Tests;

public static class TestHelpers
{
    /// <summary>
    /// Creates a fresh in-memory database for each test.
    /// Using a unique name per call ensures tests don't share state.
    /// </summary>
    public static AppDbContext CreateDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Provides a minimal IConfiguration with JWT settings for AuthService.
    /// </summary>
    public static IConfiguration CreateJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]              = "super-secret-test-key-that-is-long-enough-32chars",
                ["Jwt:Issuer"]           = "ClubManagementApi",
                ["Jwt:Audience"]         = "ClubManagementApi",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();
}
