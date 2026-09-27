using CustomerService.Contracts;
using CustomerService.Domain;
using CustomerService.Persistence;

namespace CustomerService.Application;

/// <summary>
/// Logique métier. Ne dépend ni d'ASP.NET ni d'EF Core directement → testable unitairement.
/// </summary>
public sealed class CustomerAppService(ICustomerRepository repository, TimeProvider timeProvider) : ICustomerAppService
{
    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customers = await repository.GetAllAsync(cancellationToken);
        return customers.Select(CustomerResponse.From).ToList();
    }

    public async Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : CustomerResponse.From(customer);
    }

    public async Task<CreateCustomerResult> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = Customer.NormalizeEmail(request.Email);
        if (await repository.EmailExistsAsync(email, cancellationToken))
        {
            return CreateCustomerResult.EmailAlreadyUsed();
        }

        var customer = Customer.Create(request.Name, email, timeProvider.GetUtcNow().UtcDateTime);
        await repository.AddAsync(customer, cancellationToken);

        return CreateCustomerResult.Success(CustomerResponse.From(customer));
    }
}
