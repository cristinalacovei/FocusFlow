using System.ComponentModel.DataAnnotations;

namespace FocusFlow.Api.Models
{
    public class Activity
    {
        public int Id { get; set; } 

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Ex: "Coding (Backend)", "Studat pentru examen"

        // --- Relația cu Utilizatorul ---

        // Aceasta este cheia externă (Foreign Key)
        public string AppUserId { get; set; }

        // Aceasta este "proprietatea de navigare"
        // Îi spune lui EF Core că o Activitate aparține unui singur AppUser
        public AppUser User { get; set; }
    }
}