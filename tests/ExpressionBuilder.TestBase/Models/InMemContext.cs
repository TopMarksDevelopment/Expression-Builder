using Microsoft.EntityFrameworkCore;

namespace ExpressionBuilder.Tests.Models;

public class EbContext(DbContextOptions<EbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Brand> Brands { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<StockLocation> StockLocations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(e =>
        {
            e.HasMany(c => c.Products)
                .WithMany(p => p.Categories)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductCategory",
                    l =>
                        l.HasOne<Product>()
                            .WithMany()
                            .HasForeignKey("ProductId"),
                    r =>
                        r.HasOne<Category>()
                            .WithMany()
                            .HasForeignKey("CategoryId"),
                    j => j.HasKey("ProductId", "CategoryId")
                );
        });

        builder.Entity<Product>(e =>
        {
            e.HasOne(c => c.Category).WithMany();

            e.HasMany(x => x.StockLocations).WithOne(x => x.Product);

            e.HasOne(x => x.Brand).WithMany();
        });

        builder.Entity<Brand>(e =>
        {
            e.HasOne(x => x.Supplier).WithMany();
        });
    }
}
