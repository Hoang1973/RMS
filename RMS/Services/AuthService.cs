using Microsoft.CodeAnalysis.Scripting;
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

            if (user == null || string.IsNullOrEmpty(user.Password) || !IsValidBCryptHash(user.Password))
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return null;
            }

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

        private bool IsValidBCryptHash(string hash)
        {
            try
            {
                return !string.IsNullOrEmpty(hash) &&
                       (hash.StartsWith("$2a$") || hash.StartsWith("$2b$")) &&
                       hash.Length >= 59;
            }
            catch
            {
                return false;
            }
        }
    }

}
