using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.API.Entities
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(65535)]
        public string Description { get; set; }

        [ForeignKey("AuthorId")]
        public Author Author { get; set; }

        public Guid AuthorId { get; set; }

    }
}
