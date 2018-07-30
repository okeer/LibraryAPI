using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
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

            if (bookForCreation.Title == bookForCreation.Description)
                ModelState.AddModelError(nameof(BookCreationDto), "Title and Description fields should not be the same.");

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

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

        [HttpPut("{bookId}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid bookId, [FromBody] BookCreationDto updatedBookDto)
        {
            if (updatedBookDto == null)
                return BadRequest();

            if (!_repo.IsAuthorExists(authorId))
                return NotFound();

            var bookEntity = _repo.GetBookForAuthor(bookId, authorId);
            if (bookEntity == null)
            {
                bookEntity = Mapper.Map<Book>(updatedBookDto);
                bookEntity.Id = bookId;
                _repo.AddBookForAuthor(bookEntity, authorId);
                if (!_repo.SaveContext())
                    throw new Exception("DB error while upserting a book resource");

                var bookToReturn = Mapper.Map<Book, BookDto>(bookEntity);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, bookId = bookId }, bookToReturn);
            }

            Mapper.Map(updatedBookDto, bookEntity);
            _repo.UpdateBook(bookEntity);
            if (!_repo.SaveContext())
                throw new Exception("An error occurred while updating a book resource");

            return NoContent();
        }

        [HttpPatch("{bookId}")]
        public IActionResult PatchBookForAuthor(Guid authorId, Guid bookId, [FromBody] JsonPatchDocument<BookCreationDto> jsonPatch)
        {
            if (jsonPatch == null)
                return BadRequest();

            if (!_repo.IsAuthorExists(authorId))
                return NotFound();

            var bookEntity = _repo.GetBookForAuthor(bookId, authorId);
            if (bookEntity == null)
            {
                var bookToCreate = new BookCreationDto();
                jsonPatch.ApplyTo(bookToCreate);

                var bookEntityToAdd = Mapper.Map<Book>(bookToCreate);
                bookEntityToAdd.Id = bookId;

                _repo.AddBookForAuthor(bookEntityToAdd, authorId);

                if (!_repo.SaveContext())
                    throw new Exception("err");

                return CreatedAtRoute("GetBookForAuthor", new { authorId, bookId }, bookToCreate);
            }

            var bookCreationDto = Mapper.Map<BookCreationDto>(bookEntity);
            jsonPatch.ApplyTo(bookCreationDto);

            if (!ModelState.IsValid)
                throw new Exception("Something went wrong");

            Mapper.Map(bookCreationDto, bookEntity);
            _repo.UpdateBook(bookEntity);
            if (!_repo.SaveContext())
                throw new Exception("Error while saving crx");

            return NoContent();
        }
    }
}
