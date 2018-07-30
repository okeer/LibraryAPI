using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private LibraryContext _ctx;
        private IPropertyMappingService _mappingService;

        public LibraryRepository(LibraryContext ctx, IPropertyMappingService mappingService)
        {
            _ctx = ctx;
            _mappingService = mappingService;
        }

        public void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();
            _ctx.Authors.Add(author);

            if (author.Books.Any())
                foreach (var book in author.Books)
                    book.Id = Guid.NewGuid();
        }

        public void AddBookForAuthor(Book book, Guid authorId)
        {
            var author = GetAuthor(authorId);
            if (author != null)
            {
                if (book.Id == Guid.Empty)
                    book.Id = Guid.NewGuid();
                author.Books.Add(book);
            }
        }

        public void DeleteAuthor(Author author)
        {
            _ctx.Authors.Remove(author);
        }

        public void DeleteBook(Book book)
        {
            _ctx.Books.Remove(book);
        }

        public Author GetAuthor(Guid authorId)
        {
            return _ctx.Authors.FirstOrDefault(x => x.Id == authorId);
        }

        public IEnumerable<Author> GetAuthors()
        {
            return _ctx.Authors
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters resourceParameters)
        {
            //var allAuthors = _ctx.Authors
            //    .OrderBy(x => x.FirstName)
            //    .ThenBy(x => x.LastName)
            //    .AsQueryable();

            var allAuthors = _ctx.Authors.ApplySort(resourceParameters.OrderBy, _mappingService.GetPropertyMapping<AuthorDto, Author>());

            if (!string.IsNullOrEmpty(resourceParameters.SearchQuery))
            {
                var query = resourceParameters.SearchQuery.Trim().ToLowerInvariant();
                allAuthors = allAuthors.Where(x => x.FirstName.ToLowerInvariant().Contains(query) || x.LastName.ToLowerInvariant().Contains(query));
            }

            return PagedList<Author>.Create(allAuthors, resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return _ctx.Authors.Where(x => authorIds.Contains(x.Id))
                .OrderBy(x => x.FirstName)
                .OrderBy(x => x.LastName)
                .ToList();
        }

        public Book GetBookForAuthor(Guid bookId, Guid authorId)
        {
            return _ctx.Books.Where(x => x.AuthorId == authorId && x.Id == bookId).FirstOrDefault();
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
        {
            return _ctx.Books.Where(x => x.AuthorId == authorId).ToList();
        }

        public bool IsAuthorExists(Guid authorId)
        {
            return GetAuthor(authorId) == null ? false : true;
        }

        public bool IsBookExistsFromAuthor(Guid bookId, Guid authorId)
        {
            return _ctx.Books.Where(x => x.AuthorId == authorId && x.Id == bookId).FirstOrDefault() != null;
        }

        public bool SaveContext()
        {
            return _ctx.SaveChanges() >= 0;
        }

        public void UpdateAuthor(Author author)
        {
            //EF controlled
        }

        public void UpdateBook(Book book)
        {
            //EF controlled
        }
    }
}
