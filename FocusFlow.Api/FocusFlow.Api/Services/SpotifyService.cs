using FocusFlow.Api.DTO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FocusFlow.Api.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public SpotifyService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetNewAccessTokenAsync(string refreshToken)
        {
            var clientId = _config["Spotify:ClientId"];
            var clientSecret = _config["Spotify:ClientSecret"];
            var client = _httpClientFactory.CreateClient();

            // Pregătim datele pentru cererea de refresh
            var requestData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            // Autorizarea Basic (la fel ca la callback)
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // Facem cererea POST
            var response = await client.PostAsync("https://accounts.spotify.com/api/token", requestData);

            if (!response.IsSuccessStatusCode)
            {
                // A eșuat (ex: refresh_token a expirat sau a fost revocat)
                throw new Exception("Eroare la reîmprospătarea token-ului Spotify.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponseDto>();

            // Returnăm noul access_token (de obicei valid 1 oră)
            return tokenResponse?.AccessToken ?? throw new Exception("Răspuns invalid la refresh token.");
        }

        public async Task<string> SearchPlaylistAsync(string accessToken, string activityName, string mood)
        {
            // 1. Creăm un termen de căutare
            // Exemplu: "lofi beats" sau "coding electronic"
            string searchQuery = $"{activityName} {mood}";
            if (activityName.ToLower().Contains("read"))
            {
                searchQuery = "instrumental focus playlist"; // Exemplu de logică personalizată
            }

            var client = _httpClientFactory.CreateClient();
            // Autorizăm cererea cu noul Access Token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // 2. Facem cererea GET către API-ul de căutare Spotify
            var searchUrl = "https://api.spotify.com/v1/search" +
                $"?q={Uri.EscapeDataString(searchQuery)}" +
                "&type=playlist" +
                "&limit=1"; // Vrem doar un rezultat (cel mai relevant)

            var response = await client.GetAsync(searchUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Eroare la căutarea playlist-ului pe Spotify.");
            }

            // 3. Extragem URI-ul playlist-ului din răspunsul JSON
            // (Acesta este un mod simplificat de a parcurge JSON-ul)
            using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var playlists = jsonDoc.RootElement.GetProperty("playlists").GetProperty("items");

            if (playlists.GetArrayLength() == 0)
            {
                // Nu am găsit nimic, returnăm un playlist generic
                return "spotify:playlist:37i9dQZF1DXcBWIGjsY2M4"; // (Exemplu: Lo-Fi Beats)
            }

            var playlistUri = playlists[0].GetProperty("uri").GetString();
            return playlistUri ?? "spotify:playlist:37i9dQZF1DXcBWIGjsY2M4"; // Fallback
        }
    }
}