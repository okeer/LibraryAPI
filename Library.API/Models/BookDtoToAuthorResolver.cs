using AutoMapper;
using Library.API.Entities;

namespace Library.API.Models
{
    public class BookDtoToAuthorResolver : IValueResolver<Book, BookDto, string>
    {
        public string Resolve(Book source, BookDto destination, string destMember, ResolutionContext context)
        {
            return $"{source.Author.FirstName} {source.Author.LastName}";
        }
    }
}
