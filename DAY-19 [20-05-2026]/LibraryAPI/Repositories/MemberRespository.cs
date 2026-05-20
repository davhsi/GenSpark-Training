using LibraryAPI.Data;
using LibraryAPI.Interfaces;
using LibraryAPI.Models;
namespace LibraryAPI.Repositories;

public class MemberRespository : IMemberRepository
{
    private readonly LibraryContext _context;

    public MemberRespository(LibraryContext context)
    {
        _context = context;
    }

    public List<Member> GetAllMembers()
    {
        return _context.Members.ToList();
    }

    public Member? GetMemberById(int id)
    {
        return _context.Members.Find(id);
    }

    public void AddMember(Member member)
    {
        _context.Members.Add(member);
        _context.SaveChanges();
    }


}
