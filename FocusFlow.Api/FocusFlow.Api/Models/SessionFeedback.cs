using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Api.Models
{
    public class SessionFeedback
    {
        public int Id { get; set; } // Cheia primară

        [Required]
        [Range(1, 5)]
        public int ProductivityRating { get; set; } // Rating-ul de productivitate (1-5 stele)

        [MaxLength(50)]
        public string MusicFeedback { get; set; } // Ex: "Ajutat", "Neutru", "Distras"

        public string? MusicGenreUsed { get; set; } // Ex: "lo-fi", "synthwave" (salvat de noi)

        // --- Relația cu Sesiunea (Feedback-ul cui?) ---
        [Required]
        public int FocusSessionId { get; set; }
        public FocusSession FocusSession { get; set; }
    }
}