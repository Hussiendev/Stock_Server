using Moq;
using Stock_Server.Controllers;
using Stock_Server.Mapper;          // for JSONProduct and UpdateJSONProduct
using Stock_Server.Service;
using Stock_Server.Util.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Stock_Server.Tests.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _controller = new ProductController(_mockService.Object);
    }

    [Fact]
    public async Task GetByName_ReturnsOk_WhenProductExists()
    {
        var productName = "TestProduct";
        var expectedProduct = new JSONProduct { Name = productName };
        _mockService.Setup(s => s.GetProductByNameAsync(productName))
                    .ReturnsAsync(expectedProduct);

        var result = await _controller.GetByName(productName);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<JSONProduct>(okResult.Value);
        Assert.Equal(expectedProduct.Name, returnedProduct.Name);
    }

    [Fact]
    public async Task GetByName_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var productName = "NonExistentProduct";
        _mockService.Setup(s => s.GetProductByNameAsync(productName))
                    .ThrowsAsync(new ItemNotFoundException("Not found"));

        var result = await _controller.GetByName(productName);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenProductIsCreated()
    {
        var newProduct = new JSONProduct { Name = "NewProduct" };
        var createdProduct = new JSONProduct { Id = "1", Name = "NewProduct" };
        _mockService.Setup(s => s.CreateProductAsync(newProduct))
                    .ReturnsAsync(createdProduct);

        var result = await _controller.Create(newProduct);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedProduct = Assert.IsType<JSONProduct>(createdAtActionResult.Value);
        Assert.Equal(createdProduct.Id, returnedProduct.Id);
        Assert.Equal(createdProduct.Name, returnedProduct.Name);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrowsBadRequestException()
    {
        var newProduct = new JSONProduct { Name = "InvalidProduct" };
        _mockService.Setup(s => s.CreateProductAsync(newProduct))
                    .ThrowsAsync(new BadRequestException("Invalid data"));

        var result = await _controller.Create(newProduct);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid data", badRequestResult.Value);
    }

    // ✅ Updated test for Update – now uses UpdateJSONProduct
    [Fact]
    public async Task Update_ReturnsNoContent_WhenProductIsUpdated()
    {
        var productName = "TestProduct";
        var updateRequest = new UpdateJSONProduct { Name = "UpdatedProduct" };
        _mockService.Setup(s => s.UpdateProductAsync(productName, updateRequest))
                    .Returns(Task.CompletedTask);

        var result = await _controller.Update(productName, updateRequest);

        Assert.IsType<NoContentResult>(result);
    }

    // ✅ Updated test for Update – NotFound case
    [Fact]
    public async Task Update_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var productName = "NonExistentProduct";
        var updateRequest = new UpdateJSONProduct { Name = "UpdatedProduct" };
        _mockService.Setup(s => s.UpdateProductAsync(productName, updateRequest))
                    .ThrowsAsync(new ItemNotFoundException("Not found"));

        var result = await _controller.Update(productName, updateRequest);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenProductIsDeleted()
    {
        var productId = "123";
        _mockService.Setup(s => s.DeleteProductAsync(productId))
                    .Returns(Task.CompletedTask);

        var result = await _controller.Delete(productId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var productId = "999";
        _mockService.Setup(s => s.DeleteProductAsync(productId))
                    .ThrowsAsync(new ItemNotFoundException("Not found"));

        var result = await _controller.Delete(productId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Not found", notFoundResult.Value);
    }

   


    [Fact]
    public async Task GetCapital_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var productName = "NonExistentProduct";
        _mockService.Setup(s => s.GetCapital(productName))
                    .ThrowsAsync(new ItemNotFoundException("Not found"));

        var result = await _controller.GetCapital(productName);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Not found", notFoundResult.Value);
    }
    
}