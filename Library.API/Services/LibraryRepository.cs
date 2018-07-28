using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private LibraryContext _ctx;

        public LibraryRepository(LibraryContext ctx)
        {
            _ctx = ctx;
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

        public Author GetAuthor(Guid authorId)
        {
            return _ctx.Authors.FirstOrDefault(x => x.Id == authorId);
        }

        public IEnumerable<Author> GetAuthors()
        {
            return _ctx.Authors.ToList();
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return GetAuthors().Where(x => authorIds.Contains(x.Id))
                .OrderBy(x => x.FirstName)
                .OrderBy(x => x.LastName)
                .ToList();
        }

        public Book GetBookForAuthor(Guid bookId, Guid authorId)
        {
            return GetBooksForAuthor(authorId).FirstOrDefault(x => x.Id == bookId);
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
        {
            return GetAuthor(authorId).Books.ToList();
        }

        public bool IsAuthorExists(Guid authorId)
        {
            return GetAuthor(authorId) == null ? false : true;
        }

        public bool SaveContext()
        {
            return _ctx.SaveChanges() >= 0;
        }

        public void UpdateAuthor(Author author)
        {
            throw new NotImplementedException();
        }

        public void UpdateBook(Book book)
        {
            throw new NotImplementedException();
        }
    }
}
