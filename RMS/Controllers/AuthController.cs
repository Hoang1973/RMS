using Microsoft.AspNetCore.Mvc;
using RMS.Models;
using RMS.Services;

namespace RMS.Controllers
{
    // RMS/Controllers/AuthController.cs
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.AuthenticateAsync(model);

            if (token == null)
                return Unauthorized();

            return Ok(new { token });
        }
    }

}
