using System.Text.Json.Serialization;

namespace FocusFlow.Api.DTO
{
    // Acest DTO mapează răspunsul de la Spotify (care folosește snake_case)
    public class SpotifyTokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } // Acesta este cel pe care îl vom salva!

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}