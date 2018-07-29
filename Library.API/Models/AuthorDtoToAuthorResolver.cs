using AutoMapper;
using Library.API.Entities;

namespace Library.API.Models
{
    public class AuthorDtoToAuthorResolver : IValueResolver<Author, AuthorDto, string>
    {
        public string Resolve(Author source, AuthorDto destination, string destMember, ResolutionContext context)
        {
            return $"{source.FirstName} {source.LastName}";
        }
    }
}
