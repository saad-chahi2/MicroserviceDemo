using System.ComponentModel.DataAnnotations;
using CustomerService.Contracts;

namespace CustomerService.UnitTests.Contracts;

/// <summary>
/// Vérifie les règles [Required], [EmailAddress]... que [ApiController] applique automatiquement (→ 400).
/// </summary>
public class CreateCustomerRequestValidationTests
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
        var request = new CreateCustomerRequest { Name = "Saad", Email = "saad@example.com" };

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("", "saad@example.com", nameof(CreateCustomerRequest.Name))]
    [InlineData("Saad", "", nameof(CreateCustomerRequest.Email))]
    [InlineData("Saad", "pas-un-email", nameof(CreateCustomerRequest.Email))]
    public void InvalidRequest_ReportsTheFaultyField(string name, string email, string expectedField)
    {
        var request = new CreateCustomerRequest { Name = name, Email = email };

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(expectedField));
    }

    [Fact]
    public void NameTooLong_IsRejected()
    {
        var request = new CreateCustomerRequest { Name = new string('a', 101), Email = "saad@example.com" };

        Assert.Contains(Validate(request), e => e.MemberNames.Contains(nameof(CreateCustomerRequest.Name)));
    }
}
