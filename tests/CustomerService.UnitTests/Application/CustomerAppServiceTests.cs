using CustomerService.Application;
using CustomerService.Contracts;
using CustomerService.Domain;
using CustomerService.Persistence;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace CustomerService.UnitTests.Application;

public class CustomerAppServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 10, 0, 0, TimeSpan.Zero);

    // Faux repository (NSubstitute) + fausse horloge : aucun accès base, résultat déterministe.
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly CustomerAppService _sut;

    public CustomerAppServiceTests()
    {
        _sut = new CustomerAppService(_repository, new FakeTimeProvider(Now));
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreateAsync_WithNewEmail_SavesCustomerWithCurrentTime()
    {
        var request = new CreateCustomerRequest { Name = "Saad", Email = "Saad@Example.com" };

        var result = await _sut.CreateAsync(request, Ct);

        Assert.True(result.IsSuccess);
        Assert.Equal("saad@example.com", result.Customer!.Email);
        Assert.Equal(Now.UtcDateTime, result.Customer.CreatedAt);
        await _repository.Received(1).AddAsync(
            Arg.Is<Customer>(c => c.Name == "Saad" && c.Email == "saad@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithExistingEmail_ReturnsEmailAlreadyUsedAndDoesNotSave()
    {
        _repository.EmailExistsAsync("saad@example.com", Arg.Any<CancellationToken>()).Returns(true);
        var request = new CreateCustomerRequest { Name = "Saad", Email = " SAAD@example.com " };

        var result = await _sut.CreateAsync(request, Ct);

        Assert.False(result.IsSuccess);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((Customer?)null);

        var result = await _sut.GetByIdAsync(42, Ct);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_MapsEntitiesToResponses()
    {
        var customers = new List<Customer>
        {
            Customer.Create("Alice", "alice@example.com", Now.UtcDateTime),
            Customer.Create("Bob", "bob@example.com", Now.UtcDateTime),
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(customers);

        var result = await _sut.GetAllAsync(Ct);

        Assert.Collection(
            result,
            c => Assert.Equal("Alice", c.Name),
            c => Assert.Equal("Bob", c.Name));
    }
}
