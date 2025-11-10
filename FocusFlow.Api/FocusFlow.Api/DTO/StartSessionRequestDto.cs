using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Api.DTO
{
    public class StartSessionRequestDto
    {
        [Required]
        public int ActivityId { get; set; }

        [Required]
        public int DurationMinutes { get; set; } // Ex: 25

        [Required]
        [MaxLength(50)]
        public string Mood { get; set; } // Ex: "Motivat", "Obosit"
    }
}