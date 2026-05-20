using LibraryAPI.DTOs;

namespace LibraryAPI.Interfaces;

public interface IBookService
{
    List<BookDto> GetAllBooks();
    BookDto? GetBookById(int id);
    BookDto AddBook(CreateBookDto bookDto);
    List<BookDto>? SearchBookByTitle(string title);
}
