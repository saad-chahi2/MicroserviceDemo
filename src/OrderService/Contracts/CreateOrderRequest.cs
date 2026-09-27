using System.ComponentModel.DataAnnotations;
using OrderService.Domain;

namespace OrderService.Contracts;

public sealed class CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; init; }

    [Required]
    [StringLength(Order.ProductNameMaxLength)]
    public string ProductName { get; init; } = string.Empty;

    [Range(1, Order.MaxQuantity)]
    public int Quantity { get; init; }
}
