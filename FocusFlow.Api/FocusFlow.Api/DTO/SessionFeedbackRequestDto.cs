using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Api.DTO
{
    public class SessionFeedbackRequestDto
    {
        [Required]
        public int SessionId { get; set; } // De ce sesiune e legat feedback-ul

        [Required]
        [Range(1, 5)]
        public int ProductivityRating { get; set; } // 1-5 stele

        [Required]
        [MaxLength(50)]
        public string MusicFeedback { get; set; } // "Ajutat", "Neutru", "Distras"
    }
}