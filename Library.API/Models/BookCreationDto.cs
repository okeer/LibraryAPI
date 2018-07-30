using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class BookCreationDto
    {
        [Required(ErrorMessage = "Boo, give mae a title")]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
