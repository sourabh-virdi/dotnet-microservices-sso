using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Seed initial data
        SeedProducts(modelBuilder);
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        var products = new[]
        {
            new Product
            {
                Id = 1,
                Name = "Laptop Pro 15",
                Description = "High-performance laptop with 15-inch display",
                Price = 1299.99m,
                Category = "Electronics",
                SKU = "LP15-001",
                StockQuantity = 50,
                IsAvailable = true,
                ImageUrl = "/images/laptop-pro-15.jpg",
                Weight = 2.1m,
                Brand = "TechCorp",
                LaunchDate = new DateTime(2024, 1, 15),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with precision tracking",
                Price = 49.99m,
                Category = "Accessories",
                SKU = "WM-002",
                StockQuantity = 200,
                IsAvailable = true,
                ImageUrl = "/images/wireless-mouse.jpg",
                Weight = 0.12m,
                Brand = "PeripheralCorp",
                LaunchDate = new DateTime(2024, 2, 1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "USB-C Hub",
                Description = "Multi-port USB-C hub with HDMI and power delivery",
                Price = 79.99m,
                Category = "Accessories",
                SKU = "USBC-003",
                StockQuantity = 75,
                IsAvailable = true,
                ImageUrl = "/images/usb-c-hub.jpg",
                Weight = 0.3m,
                Brand = "ConnectTech",
                LaunchDate = new DateTime(2024, 1, 20),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
} 