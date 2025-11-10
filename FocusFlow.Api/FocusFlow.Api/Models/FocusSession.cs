using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Api.Models
{
    public class FocusSession
    {
        public int Id { get; set; } // Cheia primară

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; } // Nullable, pentru că sesiunea e în desfășurare

        [Required]
        public int IntendedDurationMinutes { get; set; } // Durata planificată (ex: 25 min)

        [MaxLength(50)]
        public string? Mood { get; set; } // Starea de spirit (ex: "Motivat", "Obosit")

        // --- Relația cu Utilizatorul (Cine a făcut sesiunea) ---
        [Required]
        public string AppUserId { get; set; }
        public AppUser User { get; set; }

        // --- Relația cu Activitatea (Ce a făcut) ---
        [Required]
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }

        // --- Relația cu Feedback-ul (Cum a mers) ---
        public SessionFeedback? Feedback { get; set; } // O sesiune poate avea un feedback
    }
}