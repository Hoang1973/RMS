using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using RMS.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace RMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly RMSDbContext _context;

        public AuthController(IAuthService authService, RMSDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // GET: Auth/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var token = await _authService.AuthenticateAsync(model);
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Lấy user từ database để lấy role
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            // Tạo claims cho user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim("FullName", user.Name),
                new Claim("JWT", token),
                new Claim(ClaimTypes.Role, user.UserRole.ToString())
            };

            // Tạo identity và principal
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            // Đăng nhập user
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // GET: Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: Auth/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
