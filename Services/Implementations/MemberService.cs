using ClubManagementApi.Data;
using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubManagementApi.Services.Implementations;

public class MemberService(AppDbContext db) : IMemberService
{
    public async Task<IEnumerable<MemberResponse>> GetAllAsync()
    {
        return await db.Members
            .OrderBy(m => m.FullName)
            .Select(m => ToResponse(m))
            .ToListAsync();
    }

    public async Task<MemberResponse> GetByIdAsync(int id)
    {
        var member = await db.Members.FindAsync(id)
            ?? throw new KeyNotFoundException($"Member {id} not found.");

        return ToResponse(member);
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, int requestingMemberId, string requestingRole)
    {
        var member = await db.Members.FindAsync(id)
            ?? throw new KeyNotFoundException($"Member {id} not found.");

        // Members can only update themselves; Admins can update anyone
        if (requestingRole != "Admin" && requestingMemberId != id)
            throw new UnauthorizedAccessException("You can only update your own profile.");

        member.FullName = request.FullName;
        await db.SaveChangesAsync();

        return ToResponse(member);
    }

    public async Task DeleteAsync(int id, string requestingRole)
    {
        if (requestingRole != "Admin")
            throw new UnauthorizedAccessException("Only admins can delete members.");

        var member = await db.Members.FindAsync(id)
            ?? throw new KeyNotFoundException($"Member {id} not found.");

        db.Members.Remove(member);
        await db.SaveChangesAsync();
    }

    private static MemberResponse ToResponse(Models.Entities.Member m) =>
        new(m.Id, m.FullName, m.Email, m.Role, m.CreatedAt);
}
