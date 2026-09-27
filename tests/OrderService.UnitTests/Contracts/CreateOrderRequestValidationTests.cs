using System.ComponentModel.DataAnnotations;
using OrderService.Contracts;

namespace OrderService.UnitTests.Contracts;

/// <summary>
/// Vérifie les règles [Required], [Range]... que [ApiController] applique automatiquement (→ 400).
/// </summary>
public class CreateOrderRequestValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var request = new CreateOrderRequest { CustomerId = 1, ProductName = "Clavier", Quantity = 1 };

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData(0, "Clavier", 1, nameof(CreateOrderRequest.CustomerId))]
    [InlineData(1, "", 1, nameof(CreateOrderRequest.ProductName))]
    [InlineData(1, "Clavier", 0, nameof(CreateOrderRequest.Quantity))]
    [InlineData(1, "Clavier", 1001, nameof(CreateOrderRequest.Quantity))]
    public void InvalidRequest_ReportsTheFaultyField(int customerId, string productName, int quantity, string expectedField)
    {
        var request = new CreateOrderRequest { CustomerId = customerId, ProductName = productName, Quantity = quantity };

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(expectedField));
    }
}
