using FocusFlow.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Api.Data
{
    // IMPORTANT: Moștenim din IdentityDbContext<AppUser>
    // Acest lucru adaugă automat tabelele pentru Identity (Users, Roles etc.)
    // pe lângă cele pe care le definim noi.
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aici îi spunem lui EF Core despre tabela noastră personalizată "Activities"
        public DbSet<Activity> Activities { get; set; }

        public DbSet<FocusSession> FocusSessions { get; set; }
        public DbSet<SessionFeedback> SessionFeedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relatia existentă dintre User și Activity
            builder.Entity<AppUser>()
                .HasMany(u => u.Activities)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.AppUserId)
                .OnDelete(DeleteBehavior.Cascade); // Dacă ștergi User, ștergi și Activitățile

            // Relatia 1-la-1 dintre Sesiune și Feedback
            builder.Entity<FocusSession>()
                .HasOne(s => s.Feedback)
                .WithOne(f => f.FocusSession)
                .HasForeignKey<SessionFeedback>(f => f.FocusSessionId)
                .OnDelete(DeleteBehavior.Cascade); // Dacă ștergi Sesiunea, ștergi și Feedback-ul

            // --- AICI ESTE FIX-UL ---

            // Relația dintre User și FocusSession (nou)
            builder.Entity<FocusSession>()
                .HasOne(s => s.User)
                .WithMany() // Nu avem o listă de sesiuni în AppUser, e ok
                .HasForeignKey(s => s.AppUserId)
                .OnDelete(DeleteBehavior.Cascade); // Dacă ștergi User, ștergi și Sesiunile

            // Relația dintre Activity și FocusSession (nou)
            builder.Entity<FocusSession>()
                .HasOne(s => s.Activity)
                .WithMany() // Nu avem o listă de sesiuni în Activity, e ok
                .HasForeignKey(s => s.ActivityId)
                .OnDelete(DeleteBehavior.Restrict); // <-- SOLUȚIA!
                                                    // NU șterge în cascadă. 
                                                    // Oprește ștergerea Activității dacă are Sesiuni.
        }
    }
}