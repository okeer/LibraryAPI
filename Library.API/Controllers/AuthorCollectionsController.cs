using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
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

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
                return BadRequest();

            var authorEntries = _repo.GetAuthors().Where( x => ids.Contains(x.Id));

            if (ids.Count() != authorEntries.Count())
                return NotFound();

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntries);
            return Ok(authorsToReturn);

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

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            var idsAsString = string.Join(",", authorsToReturn.Select(x => x.Id));

            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString }, authorsToReturn);
        }
    }
}
