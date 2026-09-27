using System.ComponentModel.DataAnnotations;
using CustomerService.Domain;

namespace CustomerService.Contracts;

public sealed class CreateCustomerRequest
{
    [Required]
    [StringLength(Customer.NameMaxLength)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(Customer.EmailMaxLength)]
    public string Email { get; init; } = string.Empty;
}
