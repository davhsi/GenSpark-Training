using LibraryAPI.Models;

namespace LibraryAPI.Interfaces;

public interface IMemberRepository
{
    List<Member> GetAllMembers();
    Member? GetMemberById(int id);
    void AddMember(Member member);

}
