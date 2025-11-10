namespace FocusFlow.Api.Services
{
    public interface ISpotifyService
    {
        Task<string> GetNewAccessTokenAsync(string refreshToken);
        Task<string> SearchPlaylistAsync(string accessToken, string activityName, string mood);
    }
}