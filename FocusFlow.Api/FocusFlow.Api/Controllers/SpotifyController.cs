using FocusFlow.Api.DTO;
using FocusFlow.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims; // Încă îl folosim, deși e comentat
using System.Text;

namespace FocusFlow.Api.Controllers
{
    [Route("api/spotify")]
    [ApiController]
    // [Authorize] <-- AM ȘTERS AUTORIZAREA DE PE CLASĂ
    public class SpotifyController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<SpotifyController> _logger;

        public SpotifyController(IConfiguration config, IHttpClientFactory httpClientFactory, UserManager<AppUser> userManager, ILogger<SpotifyController> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _logger = logger;
        }

        // --- ENDPOINT 0: VERIFICARE CONFIGURAȚIE (pentru debugging) ---
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var config = new
            {
                ClientId = _config["Spotify:ClientId"],
                RedirectUri = _config["Spotify:RedirectUri"],
                ClientSecretSet = !string.IsNullOrEmpty(_config["Spotify:ClientSecret"])
            };
            return Ok(config);
        }

        // --- ENDPOINT 1: TRIMITEREA LA SPOTIFY ---
        // Frontend-ul (Angular) va apela acest endpoint
        [HttpGet("login")]
        [Authorize] // <-- AM ADĂUGAT AUTORIZAREA DOAR AICI
        public IActionResult GetSpotifyAuthUrl()
        {
            var clientId = _config["Spotify:ClientId"];
            var redirectUri = _config["Spotify:RedirectUri"];
            var scopes = "streaming user-read-email user-read-private user-modify-playback-state user-read-playback-state";

            // LOGGING: Verificăm valorile din configurație
            _logger.LogInformation("=== SPOTIFY LOGIN - Generare URL ===");
            _logger.LogInformation("ClientId: {ClientId}", clientId ?? "NULL");
            _logger.LogInformation("RedirectUri din config: {RedirectUri}", redirectUri ?? "NULL");
            _logger.LogInformation("Scopes: {Scopes}", scopes);

            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("ClientId este NULL sau gol!");
            }

            if (string.IsNullOrEmpty(redirectUri))
            {
                _logger.LogError("RedirectUri este NULL sau gol!");
            }

            var spotifyAuthUrl = "https://accounts.spotify.com/authorize?" +
                                 $"response_type=code" +
                                 $"&client_id={clientId}" +
                                 $"&scope={Uri.EscapeDataString(scopes)}" +
                                 $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

            _logger.LogInformation("URL final generat: {SpotifyAuthUrl}", spotifyAuthUrl);
            _logger.LogInformation("Redirect URI encodat: {EncodedRedirectUri}", Uri.EscapeDataString(redirectUri ?? ""));

            return Ok(new { redirectUrl = spotifyAuthUrl });
        }


        // --- ENDPOINT 2: PRIMIREA RĂSPUNSULUI (CALLBACK) ---
        // Spotify va redirecționa utilizatorul AICI. 
        // Trebuie să fie PUBLIC (fără [Authorize])
        [HttpGet("callback")]
        public async Task<IActionResult> HandleSpotifyCallback(
      [FromQuery] string code,
      [FromQuery] string? error = null,
      [FromQuery(Name = "error_description")] string? errorDescription = null)
        {
            _logger.LogInformation("=== SPOTIFY CALLBACK - Primire răspuns ===");
            _logger.LogInformation("Code: {Code}", code ?? "NULL");
            _logger.LogInformation("Error: {Error}", error ?? "NULL");
            _logger.LogInformation("Error Description: {ErrorDescription}", errorDescription ?? "NULL");
            _logger.LogInformation("Query string complet: {QueryString}", Request.QueryString.ToString());

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("Spotify a returnat o eroare: {Error} - {ErrorDescription}", error, errorDescription);
                return Redirect(
                    $"http://localhost:4200/profile?spotify=error&details={Uri.EscapeDataString(errorDescription ?? error)}"
                );
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("Code-ul este NULL sau gol!");
                return Redirect("http://localhost:4200/profile?spotify=error");
            }

            // --- AICI ESTE MODIFICAREA PENTRU TESTARE ---

            // 1. Găsim utilizatorul nostru

            // DEZACTIVAT: Aceste linii vor eșua deoarece acest endpoint este public
            // și nu primește un token JWT. 'User' va fi null.
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var user = await _userManager.FindByIdAsync(userId);

            // ACTIVAT PENTRU TEST: Hardcodăm ID-ul utilizatorului.
            // Intră în SSMS și rulează: SELECT Id, UserName FROM dbo.AspNetUsers
            // Copiază ID-ul (GUID-ul) utilizatorului tău de test aici.
            var testUserId = "7d2c932c-7345-4906-9ddd-e320101f8a00"; // <--- ÎNLOCUIEȘTE ASTA CU ID-UL TĂU!
            var user = await _userManager.FindByIdAsync(testUserId);

            // --- Sfârșitul modificării ---

            if (user == null)
            {
                // Această eroare apare acum dacă ID-ul hardcodat este greșit.
                return Unauthorized("Utilizator invalid (verifică ID-ul hardcodat în SpotifyController).");
            }

            // 2. Pregătim cererea server-la-server pentru a schimba "code" pe "token"
            var clientId = _config["Spotify:ClientId"];
            var clientSecret = _config["Spotify:ClientSecret"];
            var redirectUri = _config["Spotify:RedirectUri"];

            // LOGGING: Verificăm valorile pentru token exchange
            _logger.LogInformation("=== TOKEN EXCHANGE - Pregătire cerere ===");
            _logger.LogInformation("ClientId: {ClientId}", clientId ?? "NULL");
            _logger.LogInformation("ClientSecret: {ClientSecret}", string.IsNullOrEmpty(clientSecret) ? "NULL" : "***SETAT***");
            _logger.LogInformation("RedirectUri: {RedirectUri}", redirectUri ?? "NULL");
            _logger.LogInformation("Code primit: {Code}", code);

            var client = _httpClientFactory.CreateClient();

            var requestData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            });

            // Logăm conținutul cererii (fără secret)
            var requestBody = await requestData.ReadAsStringAsync();
            _logger.LogInformation("Request body: {RequestBody}", requestBody);

            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // 3. Facem cererea POST
            _logger.LogInformation("Trimitere cerere POST către: https://accounts.spotify.com/api/token");
            var response = await client.PostAsync("https://accounts.spotify.com/api/token", requestData);

            _logger.LogInformation("Status Code: {StatusCode}", response.StatusCode);
            _logger.LogInformation("IsSuccessStatusCode: {IsSuccess}", response.IsSuccessStatusCode);

            if (!response.IsSuccessStatusCode)
            {
                // Logăm răspunsul de eroare de la Spotify
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("=== EROARE LA TOKEN EXCHANGE ===");
                _logger.LogError("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogError("Răspuns de la Spotify: {ErrorContent}", errorContent);
                
                // Spotify a refuzat cererea de token
                return Redirect($"http://localhost:4200/profile?spotify=error&details={Uri.EscapeDataString(errorContent)}");
            }

            // 4. Citim și salvăm token-urile
            var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponseDto>();

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                return Redirect("http://localhost:4200/profile?spotify=error");
            }

            // 5. SALVĂM REFRESH TOKEN-UL ÎN BAZA DE DATE
            user.SpotifyRefreshToken = tokenResponse.RefreshToken;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                // Eroare la salvarea în baza de date
                return Redirect("http://localhost:4200/profile?spotify=error");
            }

            // 6. GATA! Redirecționăm utilizatorul înapoi la aplicația Angular
            return Redirect("http://localhost:4200/profile?spotify=success");
        }
    }
}