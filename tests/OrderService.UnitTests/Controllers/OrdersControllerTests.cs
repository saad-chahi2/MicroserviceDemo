using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OrderService.Application;
using OrderService.Contracts;
using OrderService.Controllers;

namespace OrderService.UnitTests.Controllers;

public class OrdersControllerTests
{
    private readonly IOrderAppService _service = Substitute.For<IOrderAppService>();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_service);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetById_WhenOrderExists_Returns200WithOrder()
    {
        var order = new OrderResponse(1, 1, "Clavier", 2, DateTime.UtcNow);
        _service.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        var response = await _sut.GetById(1, Ct);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(order, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenOrderDoesNotExist_Returns404()
    {
        _service.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((OrderResponse?)null);

        var response = await _sut.GetById(99, Ct);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task Create_Returns201PointingToGetById()
    {
        var request = new CreateOrderRequest { CustomerId = 1, ProductName = "Clavier", Quantity = 2 };
        var created = new OrderResponse(12, 1, "Clavier", 2, DateTime.UtcNow);
        _service.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(created);

        var response = await _sut.Create(request, Ct);

        var createdAt = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(nameof(OrdersController.GetById), createdAt.ActionName);
        Assert.Equal(12, createdAt.RouteValues!["id"]);
        Assert.Equal(created, createdAt.Value);
    }
}
