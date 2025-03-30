﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models.Responses;

namespace TaskManagement.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(string email, string password, string firstName,
            string lastName, string role = "User")
        {
            // Validate input
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            // Create user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                var newRole = new ApplicationRole
                {
                    Name = role,
                    Description = $"{role} role"
                };
                await _roleManager.CreateAsync(newRole);
            }

            await _userManager.AddToRoleAsync(user, role);

            return await GenerateAuthResult(user);
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            return await GenerateAuthResult(user);
        }

        private async Task<AuthResponse> GenerateAuthResult(ApplicationUser user)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("firstName", user.FirstName),
        new Claim("lastName", user.LastName)
    };

            // Add roles
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256));

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToArray()
            };
        }

        public async Task InitializeRolesAsync()
        {
            foreach (var roleName in new[] { "Admin", "Manager", "User" })
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} role"
                    });
                }
            }
        }
    }
}
