using LibraryAPI.DTOs;
using LibraryAPI.Interfaces;
using LibraryAPI.Models;

namespace LibraryAPI.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    private static MemberDto MapToDto(Member member)
    {
        return new MemberDto
        {
            MemberId = member.MemberId,
            FullName = member.FullName,
            Email = member.Email,
            Phone = member.Phone,
            JoinDate = member.JoinDate
        };
    }

    public List<MemberDto> GetAllMembers()
    {
        return _memberRepository.GetAllMembers()
            .Select(MapToDto)
            .ToList();
    }

    public MemberDto? GetMemberById(int id)
    {
        var member = _memberRepository.GetMemberById(id);
        return member == null ? null : MapToDto(member);
    }

    public MemberDto AddMember(CreateMemberDto memberDto)
    {
        if (string.IsNullOrWhiteSpace(memberDto.FullName))
        {
            throw new ArgumentException("Member name should not be empty");
        }

        if (string.IsNullOrWhiteSpace(memberDto.Email))
        {
            throw new ArgumentException("Email should not be empty");
        }

        if (string.IsNullOrWhiteSpace(memberDto.Phone))
        {
            throw new ArgumentException("Phone Number should not be empty");
        }

        var member = new Member
        {
            FullName = memberDto.FullName,
            Email = memberDto.Email,
            Phone = memberDto.Phone,
            JoinDate = DateTime.UtcNow
        };

        _memberRepository.AddMember(member);

        return MapToDto(member);
    }
}
