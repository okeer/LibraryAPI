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

        [HttpGet("{bookId}")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!_repo.IsAuthorExists(authorId))
                return NotFound();

            var bookFromRepo = _repo.GetBookForAuthor(bookId, authorId);
            if (bookFromRepo == null)
                return NotFound();

            return Ok(Mapper.Map<Book, BookDto>(bookFromRepo));
        }
    }
}
