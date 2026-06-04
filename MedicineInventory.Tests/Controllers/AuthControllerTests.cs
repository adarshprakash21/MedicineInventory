using FluentAssertions;
using MedicineInventory.Controllers;
using MedicineInventory.Models;
using MedicineInventory.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace MedicineInventory.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IInventoryStore> _mockInventoryStore;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockInventoryStore = new Mock<IInventoryStore>();
            _mockAuthService = new Mock<IAuthService>();

            _controller = new AuthController(_mockConfiguration.Object, _mockInventoryStore.Object, _mockAuthService.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Name = "testuser",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Name = "testuser",
                Password = "hashed_password",
                Token = "jwt_token_here"
            };

            _mockAuthService
                .Setup(x => x.AuthenticateUser(loginRequest))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var resultValue = okResult.Value as dynamic;
            resultValue?.token.Should().Be("jwt_token_here");
            _mockAuthService.Verify(x => x.AuthenticateUser(loginRequest), Times.Once);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Name = "testuser",
                Password = "wrongpassword"
            };

            _mockAuthService
                .Setup(x => x.AuthenticateUser(loginRequest))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = result as UnauthorizedResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Login_WithEmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Name = string.Empty,
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Username and password are required.");
        }

        [Fact]
        public async Task Login_WithEmptyPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Name = "testuser",
                Password = string.Empty
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Username and password are required.");
        }

        [Fact]
        public async Task Login_WithNullUsername_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Name = null,
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Username and password are required.");
        }

        [Fact]
        public async Task Login_WithNullPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Name = "testuser",
                Password = null
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Username and password are required.");
        }
    }
}
