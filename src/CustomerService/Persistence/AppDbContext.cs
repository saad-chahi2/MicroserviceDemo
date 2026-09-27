using CustomerService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(Customer.NameMaxLength).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(Customer.EmailMaxLength).IsRequired();
            entity.HasIndex(c => c.Email).IsUnique();
        });
    }
}
