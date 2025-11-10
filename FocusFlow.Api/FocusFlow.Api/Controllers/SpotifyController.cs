using FocusFlow.Api.DTO;
using FocusFlow.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace FocusFlow.Api.Controllers
{
    [Route("api/spotify")]
    [ApiController]
    [Authorize] // IMPORTANT: Utilizatorul trebuie să fie logat în aplicația NOASTRĂ
                // ca să știm cui îi salvăm refresh_token-ul.
    public class SpotifyController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<AppUser> _userManager;

        public SpotifyController(IConfiguration config, IHttpClientFactory httpClientFactory, UserManager<AppUser> userManager)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
        }

        // --- ENDPOINT 1: TRIMITEREA LA SPOTIFY ---
        // Frontend-ul (Angular) va apela acest endpoint
        // GET: /api/spotify/login
        [HttpGet("login")]
        public IActionResult GetSpotifyAuthUrl()
        {
            var clientId = _config["Spotify:ClientId"];

            // Aceasta trebuie să fie IDENTICĂ cu cea pusă în Dashboard-ul Spotify
            var redirectUri = "https://localhost:7251/api/spotify/callback";

            // "Scopes" = Ce permisiuni cerem de la utilizator
            var scopes = "streaming user-read-email user-read-private user-modify-playback-state user-read-playback-state";

            // Construim URL-ul la care va fi trimis utilizatorul
            var spotifyAuthUrl = "https://accounts.spotify.com/authorize?" +
                                 $"response_type=code" +
                                 $"&client_id={clientId}" +
                                 $"&scope={Uri.EscapeDataString(scopes)}" +
                                 $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

            // Returnăm URL-ul către frontend. Frontend-ul va face redirectarea.
            return Ok(new { redirectUrl = spotifyAuthUrl });
        }


        // --- ENDPOINT 2: PRIMIREA RĂSPUNSULUI (CALLBACK) ---
        // Spotify va redirecționa utilizatorul AICI după ce acesta dă "Accept"
        // GET: /api/spotify/callback
        [HttpGet("callback")]
        public async Task<IActionResult> HandleSpotifyCallback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Spotify a returnat o eroare.");
            }

            // 1. Găsim utilizatorul nostru (cel logat)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("Utilizator invalid.");
            }

            // 2. Pregătim cererea server-la-server pentru a schimba "code" pe "token"
            var clientId = _config["Spotify:ClientId"];
            var clientSecret = _config["Spotify:ClientSecret"];
            var redirectUri = "https://localhost:7251/api/spotify/callback";

            var client = _httpClientFactory.CreateClient();

            // Pregătim datele pentru corpul cererii (form-urlencoded)
            var requestData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            });

            // Pregătim header-ul de autorizare Basic (Base64(clientId:clientSecret))
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // 3. Facem cererea POST
            var response = await client.PostAsync("https://accounts.spotify.com/api/token", requestData);

            if (!response.IsSuccessStatusCode)
            {
                // Spotify a refuzat cererea de token
                return BadRequest("Eroare la obținerea token-ului de la Spotify.");
            }

            // 4. Citim și salvăm token-urile
            var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponseDto>();

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                return BadRequest("Răspuns invalid de la Spotify.");
            }

            // 5. SALVĂM REFRESH TOKEN-UL ÎN BAZA DE DATE
            user.SpotifyRefreshToken = tokenResponse.RefreshToken;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                // Eroare la salvarea în baza de date
                return StatusCode(500, "Eroare la salvarea token-ului în baza de date.");
            }

            // 6. GATA! Redirecționăm utilizatorul înapoi la aplicația Angular
            // (Presupunând că Angular rulează pe portul 4200)
            return Redirect("http://localhost:4200/profile?spotify=success");
        }
    }
}