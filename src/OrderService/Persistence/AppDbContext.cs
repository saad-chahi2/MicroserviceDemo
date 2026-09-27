using Microsoft.EntityFrameworkCore;
using OrderService.Domain;

namespace OrderService.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.ProductName).HasMaxLength(Order.ProductNameMaxLength).IsRequired();
            entity.HasIndex(o => o.CustomerId);
        });
    }
}
