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
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _repo { get; }

        public AuthorsController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        [HttpGet()]
        public IActionResult GetAuthors()
        {
            return Ok(Mapper.Map<IEnumerable<AuthorDto>>(_repo.GetAuthors()));
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepo = _repo.GetAuthor(id);
            if (authorFromRepo == null)
                return NotFound();

            return Ok(Mapper.Map<Author, AuthorDto>(_repo.GetAuthor(id)));
        }

        [HttpPost()]
        public IActionResult AddAuthor([FromBody] AuthorCreationDto author)
        {
            if (author == null)
                return BadRequest();

            var authorEntity = Mapper.Map<Author>(author);
            _repo.AddAuthor(authorEntity);

            if (!_repo.SaveContext())
                throw new Exception("Error while creating author");
            //return StatusCode(500, "Server error");

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }

    }
}
