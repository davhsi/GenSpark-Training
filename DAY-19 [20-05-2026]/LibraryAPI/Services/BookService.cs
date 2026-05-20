using LibraryAPI.DTOs;
using LibraryAPI.Interfaces;
using LibraryAPI.Models;

namespace LibraryAPI.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            BookId = book.BookId,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublicationYear = book.PublicationYear,
            AvailableCopies = book.AvailableCopies
        };
    }

    public List<BookDto> GetAllBooks()
    {
        return _bookRepository.GetAllBooks()
            .Select(MapToDto)
            .ToList();
    }

    public BookDto? GetBookById(int id)
    {
        var book = _bookRepository.GetBookById(id);
        return book == null ? null : MapToDto(book);
    }

    public BookDto AddBook(CreateBookDto bookDto)
    {
        if (string.IsNullOrWhiteSpace(bookDto.Title))
        {
            throw new ArgumentException("Book Title should not be Empty");
        }

        if (string.IsNullOrWhiteSpace(bookDto.Author))
        {
            throw new ArgumentException("Author name should not be Empty");
        }

        if (bookDto.AvailableCopies < 0)
        {
            throw new ArgumentException("Available copies must be greater than 0");
        }

        var book = new Book
        {
            Title = bookDto.Title,
            Author = bookDto.Author,
            ISBN = bookDto.ISBN,
            PublicationYear = bookDto.PublicationYear,
            AvailableCopies = bookDto.AvailableCopies
        };

        _bookRepository.AddBook(book);

        return MapToDto(book);
    }

    public List<BookDto>? SearchBookByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return new List<BookDto>();
        }

        var books = _bookRepository.SearchBookByTitle(title);
        return books?.Select(MapToDto).ToList();
    }
}
