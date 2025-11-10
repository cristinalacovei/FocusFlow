namespace FocusFlow.Api.DTO
{
    public class StartSessionResponseDto
    {
        public int SessionId { get; set; } // ID-ul noii sesiuni salvate
        public string SpotifyPlaylistUri { get; set; } // Playlist-ul pe care să-l pornească
        public DateTime StartTime { get; set; }
    }
}