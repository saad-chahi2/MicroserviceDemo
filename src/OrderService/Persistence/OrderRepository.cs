using Microsoft.EntityFrameworkCore;
using OrderService.Domain;

namespace OrderService.Persistence;

public sealed class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<IReadOnlyList<Order>> GetAllAsync(int? customerId, CancellationToken cancellationToken)
    {
        var query = context.Orders.AsNoTracking();
        if (customerId is not null)
        {
            query = query.Where(o => o.CustomerId == customerId);
        }

        return await query.OrderBy(o => o.Id).ToListAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
    }
}
