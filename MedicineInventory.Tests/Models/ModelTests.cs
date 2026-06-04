using FluentAssertions;
using MedicineInventory.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MedicineInventory.Tests.Models
{
    public class MedicineTests
    {
        [Fact]
        public void Medicine_WithValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = new DateOnly(2025, 12, 31),
                Notes = "Pain reliever"
            };

            // Assert
            medicine.Id.Should().NotBeEmpty();
            medicine.FullName.Should().Be("Aspirin");
            medicine.Brand.Should().Be("Bayer");
            medicine.Quantity.Should().Be(100);
            medicine.Price.Should().Be(5.99m);
            medicine.Notes.Should().Be("Pain reliever");
        }

        [Fact]
        public void Medicine_WithoutNotes_CreatesSuccessfully()
        {
            // Arrange & Act
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = "Ibuprofen",
                Brand = "Advil",
                Quantity = 50,
                Price = 7.99m,
                ExpiryDate = new DateOnly(2025, 12, 31)
            };

            // Assert
            medicine.Notes.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Medicine_WithEmptyOrNullFullName_IsInvalid(string? fullName)
        {
            // Arrange
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = fullName ?? string.Empty,
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = new DateOnly(2025, 12, 31)
            };

            var context = new ValidationContext(medicine);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(medicine, context, results, true);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void Medicine_WithNegativeQuantity_IsInvalid()
        {
            // Arrange
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = -10,
                Price = 5.99m,
                ExpiryDate = new DateOnly(2025, 12, 31)
            };

            var context = new ValidationContext(medicine);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(medicine, context, results, true);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void Medicine_WithNegativePrice_IsInvalid()
        {
            // Arrange
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = -5.99m,
                ExpiryDate = new DateOnly(2025, 12, 31)
            };

            var context = new ValidationContext(medicine);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(medicine, context, results, true);

            // Assert
            isValid.Should().BeFalse();
        }
    }

    public class UserTests
    {
        [Fact]
        public void User_WithValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var user = new User
            {
                Id = 1,
                Name = "john_doe",
                Password = "hashed_password_here"
            };

            // Assert
            user.Id.Should().NotBe(0);
            user.Name.Should().Be("john_doe");
            user.Password.Should().Be("hashed_password_here");
        }

        [Fact]
        public void User_WithToken_UpdatesSuccessfully()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "john_doe",
                Password = "hashed_password"
            };

            var token = "jwt_token_here";

            // Act
            user.Token = token;

            // Assert
            user.Token.Should().Be(token);
        }
    }

    public class CreateMedicineRequestTests
    {
        [Fact]
        public void CreateMedicineRequest_WithValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var request = new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                Notes = "Common pain reliever"
            };

            // Assert
            request.FullName.Should().Be("Aspirin");
            request.Brand.Should().Be("Bayer");
            request.Quantity.Should().Be(100);
            request.Price.Should().Be(5.99m);
            request.Notes.Should().Be("Common pain reliever");
        }
    }

    public class UserLoginRequestTests
    {
        [Fact]
        public void UserLoginRequest_WithValidCredentials_CreatesSuccessfully()
        {
            // Arrange & Act
            var request = new UserLoginRequest
            {
                Name = "john_doe",
                Password = "password123"
            };

            // Assert
            request.Name.Should().Be("john_doe");
            request.Password.Should().Be("password123");
        }
    }
}
