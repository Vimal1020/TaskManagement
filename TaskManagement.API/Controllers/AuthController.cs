using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Models;
using TaskManagement.API.Requests;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Identity;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] TaskRegisterRequest model)
        {
            var result = await _authService.RegisterAsync(
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName,
                model.Role);
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] TaskLoginRequest model)
        {
            var result = await _authService.LoginAsync(model.Email, model.Password);
            return Ok(result);
        }
    }
}