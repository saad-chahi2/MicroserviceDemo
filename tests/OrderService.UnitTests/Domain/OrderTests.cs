using OrderService.Domain;

namespace OrderService.UnitTests.Domain;

public class OrderTests
{
    private static readonly DateTime Now = new(2026, 9, 26, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidData_BuildsOrder()
    {
        var order = Order.Create(1, "  Clavier  ", 3, Now);

        Assert.Equal(1, order.CustomerId);
        Assert.Equal("Clavier", order.ProductName);
        Assert.Equal(3, order.Quantity);
        Assert.Equal(Now, order.OrderDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidCustomerId_Throws(int customerId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Order.Create(customerId, "Clavier", 1, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(Order.MaxQuantity + 1)]
    public void Create_WithQuantityOutOfRange_Throws(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Order.Create(1, "Clavier", quantity, Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankProductName_Throws(string productName)
    {
        Assert.Throws<ArgumentException>(() => Order.Create(1, productName, 1, Now));
    }
}
