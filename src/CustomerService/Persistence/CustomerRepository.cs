using CustomerService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Persistence;

public sealed class CustomerRepository(AppDbContext context) : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Customers.AsNoTracking().OrderBy(c => c.Id).ToListAsync(cancellationToken);

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        context.Customers.AnyAsync(c => c.Email == email, cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
    }
}
