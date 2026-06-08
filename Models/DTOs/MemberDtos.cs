namespace ClubManagementApi.Models.DTOs;

public record MemberResponse(int Id, string FullName, string Email, string Role, DateTime CreatedAt);

public record UpdateMemberRequest(string FullName);
