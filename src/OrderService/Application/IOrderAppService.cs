using OrderService.Contracts;

namespace OrderService.Application;

public interface IOrderAppService
{
    Task<IReadOnlyList<OrderResponse>> GetAllAsync(int? customerId, CancellationToken cancellationToken);

    Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}
