using OrderService.Contracts;
using OrderService.Domain;
using OrderService.Persistence;

namespace OrderService.Application;

/// <summary>
/// Logique métier. Ne dépend ni d'ASP.NET ni d'EF Core directement → testable unitairement.
/// </summary>
public sealed class OrderAppService(IOrderRepository repository, TimeProvider timeProvider) : IOrderAppService
{
    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync(int? customerId, CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(customerId, cancellationToken);
        return orders.Select(OrderResponse.From).ToList();
    }

    public async Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderResponse.From(order);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = Order.Create(request.CustomerId, request.ProductName, request.Quantity, timeProvider.GetUtcNow().UtcDateTime);
        await repository.AddAsync(order, cancellationToken);

        return OrderResponse.From(order);
    }
}
