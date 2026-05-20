using LibraryAPI.Models;

namespace LibraryAPI.Interfaces;

public interface IBookRepository
{
    List<Book> GetAllBooks();
    Book? GetBookById(int id);
    void AddBook(Book book);
    List<Book>? SearchBookByTitle(string title);

}
