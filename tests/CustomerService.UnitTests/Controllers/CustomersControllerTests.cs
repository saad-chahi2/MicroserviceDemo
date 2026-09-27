using CustomerService.Application;
using CustomerService.Contracts;
using CustomerService.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CustomerService.UnitTests.Controllers;

public class CustomersControllerTests
{
    private readonly ICustomerAppService _service = Substitute.For<ICustomerAppService>();
    private readonly CustomersController _sut;

    public CustomersControllerTests()
    {
        _sut = new CustomersController(_service);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetById_WhenCustomerExists_Returns200WithCustomer()
    {
        var customer = new CustomerResponse(1, "Saad", "saad@example.com", DateTime.UtcNow);
        _service.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(customer);

        var response = await _sut.GetById(1, Ct);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(customer, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenCustomerDoesNotExist_Returns404()
    {
        _service.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((CustomerResponse?)null);

        var response = await _sut.GetById(99, Ct);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task Create_WhenSuccessful_Returns201PointingToGetById()
    {
        var created = new CustomerResponse(7, "Saad", "saad@example.com", DateTime.UtcNow);
        var request = new CreateCustomerRequest { Name = "Saad", Email = "saad@example.com" };
        _service.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(CreateCustomerResult.Success(created));

        var response = await _sut.Create(request, Ct);

        var createdAt = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(nameof(CustomersController.GetById), createdAt.ActionName);
        Assert.Equal(7, createdAt.RouteValues!["id"]);
        Assert.Equal(created, createdAt.Value);
    }

    [Fact]
    public async Task Create_WhenEmailAlreadyUsed_Returns409()
    {
        var request = new CreateCustomerRequest { Name = "Saad", Email = "saad@example.com" };
        _service.CreateAsync(request, Arg.Any<CancellationToken>()).Returns(CreateCustomerResult.EmailAlreadyUsed());

        var response = await _sut.Create(request, Ct);

        var conflict = Assert.IsType<ConflictObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }
}
