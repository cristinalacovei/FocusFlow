using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Api.DTO
{
    public class CreateActivityDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Name cannot be over 100 characters")]
        public string Name { get; set; }
    }
}