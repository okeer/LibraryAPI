using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
        private IUrlHelper _uriHelper;

        private ILibraryRepository _repo { get; }

        public AuthorsController(ILibraryRepository repo, IUrlHelper uriHelper)
        {
            _repo = repo;
            _uriHelper = uriHelper;
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _uriHelper.Link("GetAuthors", new
                    {
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize
                    });
                case ResourceUriType.NextPage:
                    return _uriHelper.Link("GetAuthors", new
                    {
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize
                    });
                default:
                    return _uriHelper.Link("GetAuthors", new
                    {
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize
                    });
            }
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters resourceParameters)
        {
            var authorsFromRepo = _repo.GetAuthors(resourceParameters);

            if (!authorsFromRepo.Any() && !string.IsNullOrEmpty(resourceParameters.SearchQuery))
                return NotFound();

            var previousPageLink = authorsFromRepo.HasPrevious ? CreateAuthorsResourceUri(resourceParameters, ResourceUriType.PreviousPage) : null;
            var nextPageLink = authorsFromRepo.HasNext ? CreateAuthorsResourceUri(resourceParameters, ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            return Ok(Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
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

            var authorToReturn = Mapper.Map<AuthorCreationDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new { id = authorEntity.Id }, authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult ConfilictResolver(Guid id)
        {
            if (_repo.IsAuthorExists(id))
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorEntity = _repo.GetAuthor(id);
            if (authorEntity == null)
                return NotFound();

            _repo.DeleteAuthor(authorEntity);
            if (!_repo.SaveContext())
                throw new Exception("An error occurred while deleting of an author");

            return NoContent();
        }

    }
}
