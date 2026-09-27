using CustomerService.Domain;

namespace CustomerService.Contracts;

public sealed record CustomerResponse(int Id, string Name, string Email, DateTime CreatedAt)
{
    public static CustomerResponse From(Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, customer.CreatedAt);
}
