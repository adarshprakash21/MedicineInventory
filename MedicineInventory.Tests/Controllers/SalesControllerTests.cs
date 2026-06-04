using FluentAssertions;
using MedicineInventory.Controllers;
using MedicineInventory.Models;
using MedicineInventory.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Hosting;

namespace MedicineInventory.Tests.Controllers
{
    public class SalesControllerTests
    {
        private readonly Mock<InventoryStore> _mockInventoryStore;
        private readonly SalesController _controller;

        public SalesControllerTests()
        {
            _mockInventoryStore = new Mock<InventoryStore>(
                new Mock<IWebHostEnvironment>().Object
            );
            _controller = new SalesController(_mockInventoryStore.Object);
        }

        [Fact]
        public async Task Get_ReturnsOkWithSales()
        {
            // Arrange
            var sales = new List<SaleSummary>
            {
                new SaleSummary { Id = Guid.NewGuid(), MedicineId = Guid.NewGuid(), MedicineName = "Aspirin", QuantitySold = 10, SoldAtUtc = DateTime.UtcNow },
                new SaleSummary { Id = Guid.NewGuid(), MedicineId = Guid.NewGuid(), MedicineName = "Ibuprofen", QuantitySold = 5, SoldAtUtc = DateTime.UtcNow }
            };

            _mockInventoryStore
                .Setup(x => x.GetSalesAsync())
                .ReturnsAsync(sales.AsReadOnly());

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedSales = okResult.Value as IReadOnlyList<SaleSummary>;
            returnedSales.Should().HaveCount(2);
            _mockInventoryStore.Verify(x => x.GetSalesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkWithSale()
        {
            // Arrange
            var saleId = Guid.NewGuid();
            var sale = new SaleSummary
            {
                Id = saleId,
                MedicineId = Guid.NewGuid(),
                MedicineName = "Aspirin",
                QuantitySold = 10,
                SoldAtUtc = DateTime.UtcNow
            };

            _mockInventoryStore
                .Setup(x => x.GetSaleByIdAsync(saleId))
                .ReturnsAsync(sale);

            // Act
            var result = await _controller.GetById(saleId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedSale = okResult.Value as SaleSummary;
            returnedSale.Id.Should().Be(saleId);
            _mockInventoryStore.Verify(x => x.GetSaleByIdAsync(saleId), Times.Once);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var saleId = Guid.NewGuid();
            _mockInventoryStore
                .Setup(x => x.GetSaleByIdAsync(saleId))
                .ReturnsAsync((SaleSummary?)null);

            // Act
            var result = await _controller.GetById(saleId);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task Post_WithValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new CreateSaleRequest
            {
                MedicineId = Guid.NewGuid(),
                QuantitySold = 10
            };

            var saleRecord = new SaleRecord
            {
                Id = Guid.NewGuid(),
                MedicineId = request.MedicineId,
                QuantitySold = request.QuantitySold,
                SoldAtUtc = DateTime.UtcNow
            };

            _mockInventoryStore
                .Setup(x => x.AddSaleAsync(request))
                .ReturnsAsync((saleRecord, (string?)null));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(SalesController.GetById));
            createdResult.RouteValues.Should().ContainKey("id");
        }

        [Fact]
        public async Task Post_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateSaleRequest
            {
                MedicineId = Guid.NewGuid(),
                QuantitySold = 1000
            };

            var errorMessage = "Insufficient stock";

            _mockInventoryStore
                .Setup(x => x.AddSaleAsync(request))
                .ReturnsAsync(((SaleRecord?)null, errorMessage));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }
    }
}
