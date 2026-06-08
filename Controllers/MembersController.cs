using System.Security.Claims;
using ClubManagementApi.Models.DTOs;
using ClubManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClubManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MembersController(IMemberService memberService) : ControllerBase
{
    private int CurrentMemberId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role)!;

    /// <summary>Get all members. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<MemberResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var members = await memberService.GetAllAsync();
        return Ok(members);
    }

    /// <summary>Get a member by id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var member = await memberService.GetByIdAsync(id);
        return Ok(member);
    }

    /// <summary>Update a member's name. Members can update themselves; Admins can update anyone.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateMemberRequest request)
    {
        var member = await memberService.UpdateAsync(id, request, CurrentMemberId, CurrentRole);
        return Ok(member);
    }

    /// <summary>Delete a member. Admin only.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await memberService.DeleteAsync(id, CurrentRole);
        return NoContent();
    }
}
