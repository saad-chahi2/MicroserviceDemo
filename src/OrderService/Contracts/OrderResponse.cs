using OrderService.Domain;

namespace OrderService.Contracts;

public sealed record OrderResponse(int Id, int CustomerId, string ProductName, int Quantity, DateTime OrderDate)
{
    public static OrderResponse From(Order order) =>
        new(order.Id, order.CustomerId, order.ProductName, order.Quantity, order.OrderDate);
}
