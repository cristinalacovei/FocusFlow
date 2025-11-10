using FocusFlow.Api.Models;

namespace FocusFlow.Api.Services
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}