using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.API.Controllers;
using TaskManagement.API.Models;
using TaskManagement.API.Requests;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models.Responses;

namespace TaskManagement.Tests
{
    [TestFixture]
    [Order(1)]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _authServiceMock;
        private AuthController _authController;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
        }

        [Test]
        [Order(1)]
        public async Task Register_ReturnsOkResult_WithAuthResponse()
        {
            // Arrange
            var registerRequest = new TaskRegisterRequest
            {
                Email = "test@test.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "User"
            };

            var expectedResponse = new AuthResponse
            {
                Token = "sample.jwt.token",
                Expiration = DateTime.UtcNow.AddHours(1),
                UserId = 1,
                Email = "test@test.com",
                Roles = new[] { "User" }
            };

            _authServiceMock
                .Setup(x => x.RegisterAsync(
                    registerRequest.Email,
                    registerRequest.Password,
                    registerRequest.FirstName,
                    registerRequest.LastName,
                    registerRequest.Role))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Register(registerRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
        }

        [Test]
        [Order(2)]
        public async Task Login_ReturnsOkResult_WithAuthResponse()
        {
            // Arrange
            var loginRequest = new TaskLoginRequest
            {
                Email = "test@test.com",
                Password = "Password123!"
            };

            var expectedResponse = new AuthResponse
            {
                Token = "sample.jwt.token",
                Expiration = DateTime.UtcNow.AddHours(1),
                UserId = 1,
                Email = "test@test.com",
                Roles = new[] { "User" }
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(loginRequest.Email, loginRequest.Password))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
        }
    }
}
