using LibraryAPI.DTOs;
using LibraryAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpPost]
    public IActionResult AddBook([FromBody] CreateBookDto bookDto)
    {
        try
        {
            var createdBook = _bookService.AddBook(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.BookId }, createdBook);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetAllBooks()
    {
        var books = _bookService.GetAllBooks();
        return Ok(books);
    }

    [HttpGet("{id}")]
    public IActionResult GetBookById(int id)
    {
        var book = _bookService.GetBookById(id);
        if (book == null)
        {
            return NotFound(new { message = "Book Not Found" });
        }
        return Ok(book);
    }

    [HttpGet("search")]
    public IActionResult SearchBooks([FromQuery] string title)
    {
        var books = _bookService.SearchBookByTitle(title);
        return Ok(books);
    }
}
