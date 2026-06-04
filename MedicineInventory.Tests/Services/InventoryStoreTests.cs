using FluentAssertions;
using MedicineInventory.Models;
using MedicineInventory.Services;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Hosting;

namespace MedicineInventory.Tests.Services
{
    public class InventoryStoreTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly string _testDataPath;
        private readonly InventoryStore _inventoryStore;

        public InventoryStoreTests()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _testDataPath = Path.Combine(Path.GetTempPath(), $"medicine_inventory_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(Path.Combine(_testDataPath, "Data"));

            _mockEnvironment
                .Setup(x => x.ContentRootPath)
                .Returns(_testDataPath);

            _inventoryStore = new InventoryStore(_mockEnvironment.Object);
        }

        [Fact]
        public async Task AddMedicineAsync_WithValidRequest_AddsMedicineAndReturnsIt()
        {
            // Arrange
            var request = new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                Notes = "Common pain reliever"
            };

            // Act
            var result = await _inventoryStore.AddMedicineAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.FullName.Should().Be("Aspirin");
            result.Brand.Should().Be("Bayer");
            result.Quantity.Should().Be(100);
            result.Price.Should().Be(5.99m);
            result.Notes.Should().Be("Common pain reliever");
        }

        [Fact]
        public async Task GetMedicinesAsync_WithoutSearchTerm_ReturnsAllMedicines()
        {
            // Arrange
            var medicine1 = await _inventoryStore.AddMedicineAsync(new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            });

            var medicine2 = await _inventoryStore.AddMedicineAsync(new CreateMedicineRequest
            {
                FullName = "Ibuprofen",
                Brand = "Advil",
                Quantity = 50,
                Price = 7.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            });

            // Act
            var result = await _inventoryStore.GetMedicinesAsync(null);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(m => m.FullName == "Aspirin");
            result.Should().Contain(m => m.FullName == "Ibuprofen");
        }

        [Fact]
        public async Task GetMedicinesAsync_WithSearchTerm_ReturnsFilteredMedicines()
        {
            // Arrange
            await _inventoryStore.AddMedicineAsync(new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            });

            await _inventoryStore.AddMedicineAsync(new CreateMedicineRequest
            {
                FullName = "Ibuprofen",
                Brand = "Advil",
                Quantity = 50,
                Price = 7.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            });

            // Act
            var result = await _inventoryStore.GetMedicinesAsync("Aspirin");

            // Assert
            result.Should().HaveCount(1);
            result.First().FullName.Should().Be("Aspirin");
        }

        [Fact]
        public async Task GetMedicineByIdAsync_WithValidId_ReturnsMedicine()
        {
            // Arrange
            var medicine = await _inventoryStore.AddMedicineAsync(new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            });

            // Act
            var result = await _inventoryStore.GetMedicineByIdAsync(medicine.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(medicine.Id);
            result.FullName.Should().Be("Aspirin");
        }

        [Fact]
        public async Task GetMedicineByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _inventoryStore.GetMedicineByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateMedicineAsync_WithValidId_UpdatesMedicine()
        {
            // Arrange
            var medicine = await _inventoryStore.AddMedicineAsync(new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            });

            var updateRequest = new UpdateMedicineRequest
            {
                FullName = "Aspirin Updated",
                Brand = "Bayer",
                Quantity = 150,
                Price = 6.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(2))
            };

            // Act
            var result = await _inventoryStore.UpdateMedicineAsync(medicine.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Aspirin Updated");
            result.Quantity.Should().Be(150);
            result.Price.Should().Be(6.99m);
        }

        [Fact]
        public async Task UpdateMedicineAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var updateRequest = new UpdateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            };

            // Act
            var result = await _inventoryStore.UpdateMedicineAsync(Guid.NewGuid(), updateRequest);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteMedicineAsync_WithValidId_DeletesMedicine()
        {
            // Arrange
            var medicine = await _inventoryStore.AddMedicineAsync(new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            });

            // Act
            var result = await _inventoryStore.DeleteMedicineAsync(medicine.Id);
            var medicine_after = await _inventoryStore.GetMedicineByIdAsync(medicine.Id);

            // Assert
            result.Should().BeTrue();
            medicine_after.Should().BeNull();
        }

        [Fact]
        public async Task DeleteMedicineAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _inventoryStore.DeleteMedicineAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            // Cleanup
            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, true);
            }
        }
    }
}
