using System.Collections.Generic;

namespace Library.API.Models
{
    public class AuthorCreationDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ICollection<BookCreationDto> Books { get; set; } = new List<BookCreationDto>();
    }
}
