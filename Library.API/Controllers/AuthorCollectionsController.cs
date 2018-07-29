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
    [Route("api/authorscollection")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _repo;

        public AuthorCollectionsController(ILibraryRepository repo)
        {
            _repo = repo;
        }

        [HttpPost()]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorCreationDto> authorsFromBody)
        {
            if (authorsFromBody == null)
                return BadRequest();

            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorsFromBody);

            foreach (var author in authorEntities)
            {
                _repo.AddAuthor(author);
            }

            if (!_repo.SaveContext())
                throw new Exception("An error occurred while adding a collection of authors");

            return Ok();
        }
    }
}
