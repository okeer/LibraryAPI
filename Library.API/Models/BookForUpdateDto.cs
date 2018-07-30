using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class BookForUpdateDto : BookBaseDto
    {
        [Required(ErrorMessage = "Please fill the Description field")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
