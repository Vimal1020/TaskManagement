using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models.Requests;

namespace TaskManagement.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling user authentication operations such as registration and login.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        /// <summary>
        /// Registers a new user with the provided registration details.
        /// </summary>
        /// <param name="model">The registration details.</param>
        /// <returns>An IActionResult with the result of the registration process.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] TaskRegisterRequest model)
        {
            var result = await authService.RegisterAsync(
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName,
                model.Role);
            return Ok(result);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if the credentials are valid.
        /// </summary>
        /// <param name="model">The login credentials.</param>
        /// <returns>An IActionResult with the authentication result and JWT token.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] TaskLoginRequest model)
        {
            var result = await authService.LoginAsync(model.Email, model.Password);
            return Ok(result);
        }
    }
}