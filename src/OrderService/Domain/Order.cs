namespace OrderService.Domain;

public sealed class Order
{
    public const int ProductNameMaxLength = 200;
    public const int MaxQuantity = 1000;

    // Utilisé par EF Core pour matérialiser les entités depuis la base.
    private Order()
    {
    }

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Seul moyen de créer une commande : garantit qu'elle est toujours valide.
    /// </summary>
    public static Order Create(int customerId, string productName, int quantity, DateTime orderDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(customerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, MaxQuantity);

        return new Order
        {
            CustomerId = customerId,
            ProductName = productName.Trim(),
            Quantity = quantity,
            OrderDate = orderDate,
        };
    }
}
