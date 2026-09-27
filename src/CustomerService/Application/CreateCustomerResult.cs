using CustomerService.Contracts;

namespace CustomerService.Application;

public sealed record CreateCustomerResult
{
    private CreateCustomerResult(CustomerResponse? customer) => Customer = customer;

    public CustomerResponse? Customer { get; }

    public bool IsSuccess => Customer is not null;

    public static CreateCustomerResult Success(CustomerResponse customer) => new(customer);

    public static CreateCustomerResult EmailAlreadyUsed() => new(customer: null);
}
