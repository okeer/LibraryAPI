using Library.API.Entities;
using System;
using System.Collections.Generic;

namespace Library.API.Services
{
    public interface ILibraryRepository
    {
        IEnumerable<Author> GetAuthors();
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);

        Author GetAuthor(Guid authorId);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);

        bool IsAuthorExists(Guid authorId);
        bool IsBookExistsFromAuthor(Guid bookId, Guid authorId);

        IEnumerable<Book> GetBooksForAuthor(Guid authorId);

        Book GetBookForAuthor(Guid bookId, Guid authorId);
        void AddBookForAuthor(Book book, Guid authorId);
        void UpdateBook(Book book);

        bool SaveContext();
    }
}
