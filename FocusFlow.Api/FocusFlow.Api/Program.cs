// Adaugă aceste using-uri sus de tot
// <<<
using FocusFlow.Api.Data;
using FocusFlow.Api.Models;
using FocusFlow.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// >>>

var builder = WebApplication.CreateBuilder(args);

// --- Începe secțiunea de adăugat --- // <<<

// 1. Citește connection string-ul din appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Înregistrează AppDbContext-ul și spune-i să folosească SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// 3. Înregistrează .NET Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Relaxăm regulile pentru parole (DOAR PENTRU DEZVOLTARE)
    options.SignIn.RequireConfirmedAccount = false; // Nu avem încă email
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>(); // Spune-i lui Identity să folosească AppDbContext

// --- Sfârșește secțiunea de adăugat --- // <<<

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)), // Folosim cheia din appsettings
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization(); // Activează autorizarea
//

// Liniile de mai jos ar trebui să fie deja în fișierul tău
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();

app.Run();