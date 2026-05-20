using LibraryAPI.DTOs;

namespace LibraryAPI.Interfaces;

public interface IMemberService
{
    List<MemberDto> GetAllMembers();
    MemberDto? GetMemberById(int id);
    MemberDto AddMember(CreateMemberDto memberDto);
}

