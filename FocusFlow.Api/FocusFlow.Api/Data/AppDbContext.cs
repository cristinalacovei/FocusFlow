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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Aici putem defini relații mai complexe, dacă e nevoie
            // De exemplu, relația "un-la-mulți" dintre AppUser și Activity
            builder.Entity<AppUser>()
                .HasMany(u => u.Activities) // Un utilizator are mai multe activități
                .WithOne(a => a.User)       // O activitate are un singur utilizator
                .HasForeignKey(a => a.AppUserId); // Cheia externă este AppUserId
        }
    }
}