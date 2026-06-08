using ClubManagementApi.Models.DTOs;

namespace ClubManagementApi.Services.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<MemberResponse>> GetAllAsync();
    Task<MemberResponse> GetByIdAsync(int id);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, int requestingMemberId, string requestingRole);
    Task DeleteAsync(int id, string requestingRole);
}
