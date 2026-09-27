using OrderService.Domain;

namespace OrderService.Persistence;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(int? customerId, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Order order, CancellationToken cancellationToken);
}
