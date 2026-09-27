namespace CustomerService.Domain;

public sealed class Customer
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;

    // Utilisé par EF Core pour matérialiser les entités depuis la base.
    private Customer()
    {
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Seul moyen de créer un client : garantit qu'il est toujours valide.
    /// </summary>
    public static Customer Create(string name, string email, DateTime createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new Customer
        {
            Name = name.Trim(),
            Email = NormalizeEmail(email),
            CreatedAt = createdAt,
        };
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
