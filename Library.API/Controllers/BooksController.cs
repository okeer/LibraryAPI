using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _repo;

        public BooksController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_repo.IsAuthorExists(authorId))
                return NotFound();

            return Ok(Mapper.Map<IEnumerable<Book>, IEnumerable<BookDto>>(_repo.GetBooksForAuthor(authorId)));
        }

        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!_repo.IsAuthorExists(authorId))
                return NotFound();

            var bookFromRepo = _repo.GetBookForAuthor(bookId, authorId);
            if (bookFromRepo == null)
                return NotFound();

            return Ok(Mapper.Map<Book, BookDto>(bookFromRepo));
        }

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookCreationDto bookForCreation)
        {
            if (bookForCreation == null)
                return BadRequest();

            if (!_repo.IsAuthorExists(authorId))
                return NotFound();

            var bookEntity = Mapper.Map<Book>(bookForCreation);
            _repo.AddBookForAuthor(bookEntity, authorId);

            if (!_repo.SaveContext())
                throw new Exception("An error occured while adding a book for author");

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor", new { authorId = bookEntity.AuthorId, bookId = bookEntity.Id }, bookToReturn);
        }

        [HttpDelete("{bookId}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!_repo.IsAuthorExists(authorId))
                return NotFound();

            var bookEntity = _repo.GetBookForAuthor(bookId, authorId);
            if (bookEntity == null)
                return NotFound();

            _repo.DeleteBook(bookEntity);
            if (!_repo.SaveContext())
                throw new Exception("An error has occurred while deleting of a book");

            return NoContent();
        }
    }
}
