using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RMS.Data;
using RMS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RMS.Services
{
    // RMS/Services/AuthService.cs
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(LoginModel model);
    }

    public class AuthService : IAuthService
    {
        private readonly RMSDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(RMSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> AuthenticateAsync(LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || user.Password != model.Password) // In production, use proper password hashing
                return null;

            var claims = new[]
            {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserRole.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
