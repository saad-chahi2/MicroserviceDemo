using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OrderService.Application;
using OrderService.Contracts;
using OrderService.Domain;
using OrderService.Persistence;

namespace OrderService.UnitTests.Application;

public class OrderAppServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 10, 0, 0, TimeSpan.Zero);

    // Faux repository (NSubstitute) + fausse horloge : aucun accès base, résultat déterministe.
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly OrderAppService _sut;

    public OrderAppServiceTests()
    {
        _sut = new OrderAppService(_repository, new FakeTimeProvider(Now));
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreateAsync_SavesOrderDatedNow()
    {
        var request = new CreateOrderRequest { CustomerId = 1, ProductName = "Clavier", Quantity = 2 };

        var result = await _sut.CreateAsync(request, Ct);

        Assert.Equal(Now.UtcDateTime, result.OrderDate);
        await _repository.Received(1).AddAsync(
            Arg.Is<Order>(o => o.CustomerId == 1 && o.ProductName == "Clavier" && o.Quantity == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_PassesCustomerFilterToRepository()
    {
        _repository.GetAllAsync(5, Arg.Any<CancellationToken>())
            .Returns([Order.Create(5, "Souris", 1, Now.UtcDateTime)]);

        var result = await _sut.GetAllAsync(5, Ct);

        var order = Assert.Single(result);
        Assert.Equal(5, order.CustomerId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Order?)null);

        var result = await _sut.GetByIdAsync(42, Ct);

        Assert.Null(result);
    }
}
