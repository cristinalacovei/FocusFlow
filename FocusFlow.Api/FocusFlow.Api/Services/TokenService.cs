using FocusFlow.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FocusFlow.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            // Preluăm cheia secretă din appsettings.json și o convertim
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        }

        public string CreateToken(AppUser user)
        {
            // 1. Definim "revendicările" (Claims) - ce informații punem în token
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Name, user.UserName!),
    new Claim(ClaimTypes.Email, user.Email!)
};



            // 2. Definim "semnătura" (Credentials) - cum securizăm token-ul
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 3. Creăm descrierea token-ului
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7), // Token-ul expiră în 7 zile
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            // 4. Creăm și scriem token-ul
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token); // Îl returnăm ca string
        }
    }
}