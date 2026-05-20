using LibraryAPI.DTOs;
using LibraryAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public IActionResult GetAllMembers()
    {
        var members = _memberService.GetAllMembers();
        return Ok(members);
    }

    [HttpGet("{id}")]
    public IActionResult GetMemberById(int id)
    {
        var member = _memberService.GetMemberById(id);
        if (member == null)
        {
            return NotFound(new { message = "Member Not Found" });
        }
        return Ok(member);
    }

    [HttpPost]
    public IActionResult AddMember([FromBody] CreateMemberDto memberDto)
    {
        try
        {
            var createdMember = _memberService.AddMember(memberDto);
            return CreatedAtAction(nameof(GetMemberById), new { id = createdMember.MemberId }, createdMember);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
