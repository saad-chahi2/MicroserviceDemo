using CustomerService.Contracts;

namespace CustomerService.Application;

public interface ICustomerAppService
{
    Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<CreateCustomerResult> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
}
