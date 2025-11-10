using FocusFlow.Api.Data;
using FocusFlow.Api.DTO;
using FocusFlow.Api.Models;
using FocusFlow.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FocusFlow.Api.Controllers
{
    [Route("api/session")]
    [ApiController]
    [Authorize] // Totul aici este securizat
    public class FocusSessionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ISpotifyService _spotifyService;

        public FocusSessionController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            ISpotifyService spotifyService)
        {
            _context = context;
            _userManager = userManager;
            _spotifyService = spotifyService;
        }

        // --- ENDPOINT PENTRU PORNIREA SESIUNII ---
        // POST: /api/session/start
        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromBody] StartSessionRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Găsim utilizatorul și activitatea
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var activity = await _context.Activities.FindAsync(request.ActivityId);

            // Verificări de siguranță
            if (user == null)
                return NotFound(new { error = "Utilizatorul nu a fost găsit." });
            if (activity == null || activity.AppUserId != userId)
                return NotFound(new { error = "Activitatea nu a fost găsită sau nu vă aparține." });
            if (string.IsNullOrEmpty(user.SpotifyRefreshToken))
                return BadRequest(new { error = "Contul Spotify nu este conectat." });

            string playlistUri;
            try
            {
                // 2. Logica Spotify
                // Obținem un access_token proaspăt folosind refresh_token-ul salvat
                var accessToken = await _spotifyService.GetNewAccessTokenAsync(user.SpotifyRefreshToken);

                // Căutăm un playlist bazat pe activitate și stare
                playlistUri = await _spotifyService.SearchPlaylistAsync(accessToken, activity.Name, request.Mood);
            }
            catch (Exception ex)
            {
                // Dacă Spotify eșuează, nu oprim sesiunea.
                // Logăm eroarea și oferim un playlist generic (fallback).
                Console.WriteLine($"Eroare Spotify: {ex.Message}");
                playlistUri = "spotify:playlist:37i9dQZF1DXcBWIGjsY2M4"; // Fallback Lo-Fi Beats
            }


            // 3. Creăm și salvăm sesiunea în baza de date
            var newSession = new FocusSession
            {
                StartTime = DateTime.UtcNow, // Folosim UTC pentru server
                IntendedDurationMinutes = request.DurationMinutes,
                Mood = request.Mood,
                AppUserId = userId,
                ActivityId = request.ActivityId,
                // EndTime este null (sesiunea e activă)
            };

            await _context.FocusSessions.AddAsync(newSession);
            await _context.SaveChangesAsync();

            // 4. Returnăm răspunsul către frontend
            var response = new StartSessionResponseDto
            {
                SessionId = newSession.Id,
                SpotifyPlaylistUri = playlistUri,
                StartTime = newSession.StartTime
            };

            return Ok(response);
        }

        // --- ENDPOINT PENTRU ÎNCHEIEREA SESIUNII ȘI FEEDBACK ---
        // POST: /api/session/feedback
        [HttpPost("feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] SessionFeedbackRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Găsim sesiunea originală
            var session = await _context.FocusSessions
                .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.AppUserId == userId);

            if (session == null)
                return NotFound(new { error = "Sesiunea nu a fost găsită sau nu vă aparține." });

            // 2. Verificăm dacă sesiunea are deja feedback
            if (await _context.SessionFeedbacks.AnyAsync(f => f.FocusSessionId == session.Id))
                return BadRequest(new { error = "Această sesiune are deja feedback." });

            // 3. (Opțional) Actualizăm EndTime-ul sesiunii
            session.EndTime = DateTime.UtcNow;

            // 4. Creăm și salvăm noul feedback
            var newFeedback = new SessionFeedback
            {
                ProductivityRating = request.ProductivityRating,
                MusicFeedback = request.MusicFeedback,
                FocusSessionId = request.SessionId,
                // Aici am putea salva și genul muzical dacă l-am stoca în FocusSession
                // MusicGenreUsed = session.MusicGenreUsed 
            };

            await _context.SessionFeedbacks.AddAsync(newFeedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback înregistrat cu succes." });
        }
    }
}