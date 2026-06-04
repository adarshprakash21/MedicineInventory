using BCrypt.Net;
using FluentAssertions;
using MedicineInventory.Models;
using MedicineInventory.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace MedicineInventory.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IInventoryStore> _mockInventoryStore;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockInventoryStore = new Mock<IInventoryStore>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            _mockConfiguration
                .Setup(x => x["ApplicationSettings:JWT_Secret"])
                .Returns("this-is-a-very-long-secret-key-for-jwt-token-generation");

            _mockConfiguration
                .Setup(x => x["ApplicationSettings:JWT_Issuer"])
                .Returns("MedicineInventory");

            _mockConfiguration
                .Setup(x => x["ApplicationSettings:JWT_Audience"])
                .Returns("MedicineInventoryUsers");

            _authService = new AuthService(_mockWebHostEnvironment.Object, _mockInventoryStore.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task AuthenticateUser_WithValidCredentials_ReturnsUserWithToken()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            var user = new User
            {
                Id = 1,
                Name = "testuser",
                Password = hashedPassword
            };

            var loginRequest = new UserLoginRequest
            {
                Name = "testuser",
                Password = "password123"
            };

            var data = new InventoryDataFile
            {
                Users = new List<User> { user },
                Medicines = new List<Medicine>(),
                Sales = new List<SaleRecord>()
            };

            _mockInventoryStore
                .Setup(x => x.ReadDataAsync())
                .ReturnsAsync(data);

            // Act
            var result = await _authService.AuthenticateUser(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("testuser");
            result.Token.Should().NotBeNullOrEmpty();
            _mockInventoryStore.Verify(x => x.ReadDataAsync(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateUser_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            var user = new User
            {
                Id = 1,
                Name = "testuser",
                Password = hashedPassword
            };

            var loginRequest = new UserLoginRequest
            {
                Name = "testuser",
                Password = "wrongpassword"
            };

            var data = new InventoryDataFile
            {
                Users = new List<User> { user },
                Medicines = new List<Medicine>(),
                Sales = new List<SaleRecord>()
            };

            _mockInventoryStore
                .Setup(x => x.ReadDataAsync())
                .ReturnsAsync(data);

            // Act
            var result = await _authService.AuthenticateUser(loginRequest);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateUser_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var loginRequest = new UserLoginRequest
            {
                Name = "nonexistent",
                Password = "password123"
            };

            var data = new InventoryDataFile
            {
                Users = new List<User>(),
                Medicines = new List<Medicine>(),
                Sales = new List<SaleRecord>()
            };

            _mockInventoryStore
                .Setup(x => x.ReadDataAsync())
                .ReturnsAsync(data);

            // Act
            var result = await _authService.AuthenticateUser(loginRequest);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUser_WithNewUser_ReturnsUser()
        {
            // Arrange
            var newUser = new User
            {
                Id = 1,
                Name = "newuser",
                Password = "password123"
            };

            // Act
            var result = await _authService.RegisterUser(newUser);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(newUser.Name);
        }
    }
}
