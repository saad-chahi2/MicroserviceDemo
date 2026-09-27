using CustomerService.Domain;

namespace CustomerService.UnitTests.Domain;

public class CustomerTests
{
    private static readonly DateTime Now = new(2026, 9, 26, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_TrimsNameAndNormalizesEmail()
    {
        var customer = Customer.Create("  Saad  ", "  Saad@Example.COM ", Now);

        Assert.Equal("Saad", customer.Name);
        Assert.Equal("saad@example.com", customer.Email);
        Assert.Equal(Now, customer.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(name, "saad@example.com", Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankEmail_Throws(string email)
    {
        Assert.Throws<ArgumentException>(() => Customer.Create("Saad", email, Now));
    }
}
