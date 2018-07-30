using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private IUrlHelper _uriHelper;
        private ITypeHelperService _typeHelperService;

        private ILibraryRepository _repo { get; }

        public AuthorsController(ILibraryRepository repo, IUrlHelper uriHelper, ITypeHelperService typeHelperService)
        {
            _repo = repo;
            _uriHelper = uriHelper;
            _typeHelperService = typeHelperService;
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
                case ResourceUriType.Current:
                default:
                    return _uriHelper.Link("GetAuthors", new
                    {
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize
                    });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
                links.Add(new LinkDto(_uriHelper.Link("GetAuthor", new { id = id }), "self", "GET"));
            else
                links.Add(new LinkDto(_uriHelper.Link("GetAuthor", new { id = id, fields = fields }), "self", "GET"));

            links.Add(new LinkDto(_uriHelper.Link("DeleteAuthor", new { id = id }), "delete_author", "DELETE"));
            links.Add(new LinkDto(_uriHelper.Link("CreateBookForAuthor", new { authorId = id }), "create_book_for_author", "POST"));
            links.Add(new LinkDto(_uriHelper.Link("GetBooksForAuthor", new { authorId = id }), "get_books_for_author", "GET"));

            return links; 
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters,
            bool hasNext, bool hasPrevios)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(
                CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
                links.Add(new LinkDto(
                    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "next_page", "GET"));
            if (hasPrevios)
                links.Add(new LinkDto(
                    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previos_page", "GET"));

            return links;
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters resourceParameters)
        {
            var authorsFromRepo = _repo.GetAuthors(resourceParameters);

            if (!authorsFromRepo.Any() && !string.IsNullOrEmpty(resourceParameters.SearchQuery))
                return NotFound();

            if (!_typeHelperService.TypeHasProperties<AuthorDto>(resourceParameters.Fields))
                return BadRequest();
            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var authorsDtoToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            var links = CreateLinksForAuthors(resourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);

            var shapedAuthors = authorsDtoToReturn.ShapeData(resourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(x =>
            {
                var authorAsDict = x as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDict["Id"], resourceParameters.Fields);

                authorAsDict.Add("links", authorLinks);
                return authorAsDict;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links = links
            };

            return Ok(linkedCollectionResource);
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<AuthorDto>(fields))
                return BadRequest();

            var authorFromRepo = _repo.GetAuthor(id);
            if (authorFromRepo == null)
                return NotFound();

            var authorToReturn = Mapper.Map<Author, AuthorDto>(_repo.GetAuthor(id));

            var links = CreateLinksForAuthor(id, fields);

            var linkedResourceToReturn = authorToReturn.ShapeData(fields) as IDictionary<string, object>;
            linkedResourceToReturn.Add(nameof(links), links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
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

            var links = CreateLinksForAuthor(authorEntity.Id, null);
            var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;
            linkedResourceToReturn.Add(nameof(links), links);

            return CreatedAtRoute("GetAuthor", new { id = linkedResourceToReturn["Id"] }, linkedResourceToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult ConfilictResolver(Guid id)
        {
            if (_repo.IsAuthorExists(id))
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteAuthor")]
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
