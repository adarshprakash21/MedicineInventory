using FluentAssertions;
using MedicineInventory.Controllers;
using MedicineInventory.Models;
using MedicineInventory.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MedicineInventory.Tests.Controllers
{
    public class MedicinesControllerTests
    {
        private readonly Mock<IInventoryStore> _mockInventoryStore;
        private readonly MedicinesController _controller;

        public MedicinesControllerTests()
        {
            _mockInventoryStore = new Mock<IInventoryStore>();
            _controller = new MedicinesController(_mockInventoryStore.Object);
        }

        [Fact]
        public async Task Get_WithoutSearchTerm_ReturnsOkWithMedicines()
        {
            // Arrange
            var medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), FullName = "Aspirin", Brand = "Bayer", Quantity = 100, Price = 5.99m, ExpiryDate = new DateOnly(2025, 12, 31) },
                new Medicine { Id = Guid.NewGuid(), FullName = "Ibuprofen", Brand = "Advil", Quantity = 50, Price = 7.99m, ExpiryDate = new DateOnly(2025, 12, 31) }
            };

            _mockInventoryStore
                .Setup(x => x.GetMedicinesAsync(null))
                .ReturnsAsync(medicines.AsReadOnly());

            // Act
            var result = await _controller.Get(null);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedMedicines = okResult.Value as IReadOnlyList<Medicine>;
            returnedMedicines.Should().HaveCount(2);
            _mockInventoryStore.Verify(x => x.GetMedicinesAsync(null), Times.Once);
        }

        [Fact]
        public async Task Get_WithSearchTerm_ReturnsOkWithFilteredMedicines()
        {
            // Arrange
            var medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), FullName = "Aspirin", Brand = "Bayer", Quantity = 100, Price = 5.99m, ExpiryDate = new DateOnly(2025, 12, 31) }
            };

            _mockInventoryStore
                .Setup(x => x.GetMedicinesAsync("Aspirin"))
                .ReturnsAsync(medicines.AsReadOnly());

            // Act
            var result = await _controller.Get("Aspirin");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedMedicines = okResult.Value as IReadOnlyList<Medicine>;
            returnedMedicines.Should().HaveCount(1);
            _mockInventoryStore.Verify(x => x.GetMedicinesAsync("Aspirin"), Times.Once);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithMedicine()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            var medicine = new Medicine
            {
                Id = medicineId,
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = new DateOnly(2025, 12, 31)
            };

            _mockInventoryStore
                .Setup(x => x.GetMedicineByIdAsync(medicineId))
                .ReturnsAsync(medicine);

            // Act
            var result = await _controller.GetById(medicineId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedMedicine = okResult.Value as Medicine;
            returnedMedicine.Id.Should().Be(medicineId);
            _mockInventoryStore.Verify(x => x.GetMedicineByIdAsync(medicineId), Times.Once);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            _mockInventoryStore
                .Setup(x => x.GetMedicineByIdAsync(medicineId))
                .ReturnsAsync((Medicine?)null);

            // Act
            var result = await _controller.GetById(medicineId);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Post_WithValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new CreateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            };

            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Brand = request.Brand,
                Quantity = request.Quantity,
                Price = request.Price,
                ExpiryDate = new DateOnly(2025, 12, 31)
            };

            _mockInventoryStore
                .Setup(x => x.AddMedicineAsync(request))
                .ReturnsAsync(medicine);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(MedicinesController.GetById));
            createdResult.RouteValues.Should().ContainKey("id");
        }

        [Fact]
        public async Task Put_WithValidId_ReturnsOkWithUpdatedMedicine()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            var request = new UpdateMedicineRequest
            {
                FullName = "Aspirin Updated",
                Brand = "Bayer",
                Quantity = 150,
                Price = 6.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(2))
            };

            var medicine = new Medicine
            {
                Id = medicineId,
                FullName = request.FullName,
                Brand = request.Brand,
                Quantity = request.Quantity,
                Price = request.Price,
                ExpiryDate = new DateOnly(2025, 12, 31)
            };

            _mockInventoryStore
                .Setup(x => x.UpdateMedicineAsync(medicineId, request))
                .ReturnsAsync(medicine);

            // Act
            var result = await _controller.Put(medicineId, request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Put_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            var request = new UpdateMedicineRequest
            {
                FullName = "Aspirin",
                Brand = "Bayer",
                Quantity = 100,
                Price = 5.99m,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1))
            };

            _mockInventoryStore
                .Setup(x => x.UpdateMedicineAsync(medicineId, request))
                .ReturnsAsync((Medicine?)null);

            // Act
            var result = await _controller.Put(medicineId, request);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            _mockInventoryStore
                .Setup(x => x.DeleteMedicineAsync(medicineId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(medicineId);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
            _mockInventoryStore.Verify(x => x.DeleteMedicineAsync(medicineId), Times.Once);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            _mockInventoryStore
                .Setup(x => x.DeleteMedicineAsync(medicineId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(medicineId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }
    }
}
