using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookBaseDto
    {
        [Required(ErrorMessage = "Boo, give mae a title")]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public virtual string Description { get; set; }
    }
}
