using Moq;
using Stock_Server.Controllers;
using Stock_Server.Mapper;
using Stock_Server.Service;
using Stock_Server.Util.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class SupplierControllerTests
{
    private readonly Mock<ISupplierService> _mockService;
    private readonly SupplierController _controller;

    public SupplierControllerTests()
    {
        _mockService = new Mock<ISupplierService>();
        _controller = new SupplierController(_mockService.Object);
    }

    // ✅ Corrected: method name and exception
    [Fact]
    public async Task GetByName_ReturnsOk_WhenSupplierExists()
    {
        // Arrange
        var supplierName = "Test Supplier";
        var expectedSupplier = new JSONSupplier { Name = supplierName };
        _mockService.Setup(s => s.GetSupplierByNameAsync(supplierName))
                    .ReturnsAsync(expectedSupplier);

        // Act
        var result = await _controller.GetByName(supplierName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<JSONSupplier>(okResult.Value);
        Assert.Equal(supplierName, value.Name);
    }

    // ✅ Corrected: service throws exception, not returns null
    [Fact]
    public async Task GetByName_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // Arrange
        var supplierName = "NonExistent Supplier";
        _mockService.Setup(s => s.GetSupplierByNameAsync(supplierName))
                    .ThrowsAsync(new ItemNotFoundException("Supplier not found"));

        // Act
        var result = await _controller.GetByName(supplierName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Supplier not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenSupplierIsCreated()
    {
        // Arrange
        var newSupplier = new JSONSupplier { Name = "New Supplier" };
        var createdSupplier = new JSONSupplier { Id = "1", Name = "New Supplier" };
        _mockService.Setup(s => s.CreateSupplierAsync(newSupplier))
                    .ReturnsAsync(createdSupplier);

        // Act
        var result = await _controller.Create(newSupplier);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var value = Assert.IsType<JSONSupplier>(createdAtActionResult.Value);
        Assert.Equal(createdSupplier.Id, value.Id);
        Assert.Equal(createdSupplier.Name, value.Name);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrowsBadRequestException()
    {
        // Arrange
        var newSupplier = new JSONSupplier { Name = "Invalid Supplier" };
        _mockService.Setup(s => s.CreateSupplierAsync(newSupplier))
                    .ThrowsAsync(new BadRequestException("Invalid data"));

        // Act
        var result = await _controller.Create(newSupplier);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid data", badRequestResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSupplierIsUpdated()
    {
        // Arrange
        var supplierName = "Existing Supplier";
        var updateRequest = new JSONSupplier { Name = "Updated Supplier" };
        _mockService.Setup(s => s.UpdateSupplierAsync(supplierName, updateRequest))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(supplierName, updateRequest);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // Arrange
        var supplierName = "NonExistent Supplier";
        var updateRequest = new JSONSupplier { Name = "Updated Supplier" };
        _mockService.Setup(s => s.UpdateSupplierAsync(supplierName, updateRequest))
                    .ThrowsAsync(new ItemNotFoundException("Supplier not found"));

        // Act
        var result = await _controller.Update(supplierName, updateRequest);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Supplier not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSupplierIsDeleted()
    {
        // Arrange
        var supplierId = "1";
        _mockService.Setup(s => s.DeleteSupplierAsync(supplierId))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(supplierId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // Arrange
        var supplierId = "999";
        _mockService.Setup(s => s.DeleteSupplierAsync(supplierId))
                    .ThrowsAsync(new ItemNotFoundException("Supplier not found"));

        // Act
        var result = await _controller.Delete(supplierId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Supplier not found", notFoundResult.Value);
    }
}