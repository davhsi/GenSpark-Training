using LibraryAPI.Data;
using LibraryAPI.Interfaces;
using LibraryAPI.Models;
namespace LibraryAPI.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryContext _context;
    public BookRepository(LibraryContext context)
    {
        _context = context;
    }

    public List<Book> GetAllBooks()
    {
        return _context.Books.ToList();
    }

    public Book? GetBookById(int id)
    {
        return _context.Books.Find(id);
    }


    public void AddBook(Book book)
    {
        _context.Books.Add(book);
        _context.SaveChanges();
    }

    public List<Book>? SearchBookByTitle(string title)
    {
        return _context.Books.Where(b => b.Title.ToLower().Contains(title.ToLower())).ToList();
    }


}
