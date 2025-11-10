using FocusFlow.Api.DTO;
using FocusFlow.Api.Models;
using FocusFlow.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FocusFlow.Api.Controllers
{
    [Route("api/auth")] // Adresa de bază va fi /api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;

        // "Injectăm" serviciile de care avem nevoie
        public AuthController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        // --- ENDPOINT PENTRU LOGIN ---
        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Căutăm utilizatorul după username-ul (sau email-ul) dat
            // Folosim .UserName, deci ne vom loga cu username-ul
            var user = await _userManager.FindByNameAsync(loginRequestDto.Username);

            if (user == null)
            {
                // Nu dăm detalii, din motive de securitate
                return Unauthorized("Invalid username or password");
            }

            // 2. Verificăm parola
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequestDto.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }

            // 3. Totul este OK. Generăm și returnăm un token.
            var token = _tokenService.CreateToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token
            });
        }


        // --- ENDPOINT PENTRU ÎNREGISTRARE ---
        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Creăm obiectul utilizator nou
            var appUser = new AppUser
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Email
            };

            // 2. Folosim UserManager pentru a crea utilizatorul în baza de date
            // Acesta va și hasha parola automat
            var result = await _userManager.CreateAsync(appUser, registerRequestDto.Password);

            if (!result.Succeeded)
            {
                // Dacă nu a mers (ex: parola e prea simplă, email-ul există deja),
                // returnăm erorile primite de la Identity
                return BadRequest(result.Errors);
            }

            // 3. (Opțional, dar recomandat) Adăugăm utilizatorul într-un rol
            // Momentan nu avem roluri, deci comentăm această parte
            /* var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }
            */

            // 4. Returnăm un răspuns de succes
            return Ok(new { Message = "User registered successfully" });
        }
    }
}