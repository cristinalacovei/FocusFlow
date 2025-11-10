using FocusFlow.Api.Data;
using FocusFlow.Api.DTO; 
using FocusFlow.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FocusFlow.Api.Controllers
{
    [Route("api/activities")]
    [ApiController]
    [Authorize] // Tot controller-ul este securizat
    public class ActivitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ActivitiesController(AppDbContext context)
        {
            _context = context;
        }

        // --- Metodă ajutătoare pentru a lua ID-ul utilizatorului din token ---
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);

        }


        // --- ENDPOINT GET ---
        // GET: /api/activities
        [HttpGet]
        public async Task<IActionResult> GetMyActivities()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var activities = await _context.Activities
                .Where(a => a.AppUserId == userId)
                .Select(a => new ActivityDto // Mapăm la DTO
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .ToListAsync();

            // Va returna 200 OK și o listă goală ([]) dacă nu găsește nimic,
            // ceea ce este perfect corect.
            return Ok(activities);
        }

        // --- ENDPOINT POST ---
        // POST: /api/activities
        [HttpPost]
        public async Task<IActionResult> CreateActivity([FromBody] CreateActivityDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Verificarea ta inteligentă, care prinde eroarea 400
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                // Prindem eroarea înainte să ajungă la DB și dăm un mesaj clar
                return BadRequest(new { error = "Authenticated user not found in database (stale token?).", userId });
            }

            // Dacă am ajuns aici, userExists e true, deci ID-ul e valid
            var newActivity = new Activity
            {
                Name = createDto.Name,
                AppUserId = userId
            };

            await _context.Activities.AddAsync(newActivity);
            await _context.SaveChangesAsync(); // Acum nu va mai da eroare

            var activityDto = new ActivityDto
            {
                Id = newActivity.Id,
                Name = newActivity.Name
            };

            return StatusCode(201, activityDto);
        }

        // --- ENDPOINT DELETE ---
        // DELETE: /api/activities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var userId = GetCurrentUserId();

            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
            {
                return NotFound();
            }

            // Securitate: e activitatea MEA?
            if (activity.AppUserId != userId)
            {
                return Forbid(); // 403 Forbidden - Nu e a ta
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}