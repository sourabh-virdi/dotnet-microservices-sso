using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CustomerEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            
            // Foreign key relationship
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Indexes
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
        });

        // Seed initial data
        SeedOrders(modelBuilder);
    }

    private static void SeedOrders(ModelBuilder modelBuilder)
    {
        var orders = new[]
        {
            new Order
            {
                Id = 1,
                UserId = "user123",
                CustomerName = "John Doe",
                CustomerEmail = "john.doe@example.com",
                ShippingAddress = "123 Main St, City, State 12345",
                BillingAddress = "123 Main St, City, State 12345",
                TotalAmount = 1549.98m,
                TaxAmount = 124.00m,
                ShippingCost = 25.99m,
                SubTotal = 1399.99m,
                Status = OrderStatus.Delivered,
                PaymentMethod = "Credit Card",
                PaymentReference = "CC-123456789",
                ShippedDate = DateTime.UtcNow.AddDays(-5),
                DeliveredDate = DateTime.UtcNow.AddDays(-2),
                TrackingNumber = "TRK123456789",
                Notes = "Delivered successfully",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Order
            {
                Id = 2,
                UserId = "user456",
                CustomerName = "Jane Smith",
                CustomerEmail = "jane.smith@example.com",
                ShippingAddress = "456 Oak Ave, Town, State 67890",
                BillingAddress = "456 Oak Ave, Town, State 67890",
                TotalAmount = 129.98m,
                TaxAmount = 10.40m,
                ShippingCost = 9.99m,
                SubTotal = 109.59m,
                Status = OrderStatus.Processing,
                PaymentMethod = "PayPal",
                PaymentReference = "PP-987654321",
                Notes = "Express shipping requested",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var orderItems = new[]
        {
            // Items for Order 1
            new OrderItem
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1,
                ProductName = "Laptop Pro 15",
                ProductSku = "LP15-001",
                Quantity = 1,
                UnitPrice = 1299.99m,
                TotalPrice = 1299.99m,
                ProductImageUrl = "/images/laptop-pro-15.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new OrderItem
            {
                Id = 2,
                OrderId = 1,
                ProductId = 3,
                ProductName = "USB-C Hub",
                ProductSku = "USBC-003",
                Quantity = 1,
                UnitPrice = 79.99m,
                TotalPrice = 79.99m,
                ProductImageUrl = "/images/usb-c-hub.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new OrderItem
            {
                Id = 3,
                OrderId = 1,
                ProductId = 2,
                ProductName = "Wireless Mouse",
                ProductSku = "WM-002",
                Quantity = 1,
                UnitPrice = 49.99m,
                TotalPrice = 49.99m,
                ProductImageUrl = "/images/wireless-mouse.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            // Items for Order 2
            new OrderItem
            {
                Id = 4,
                OrderId = 2,
                ProductId = 2,
                ProductName = "Wireless Mouse",
                ProductSku = "WM-002",
                Quantity = 2,
                UnitPrice = 49.99m,
                TotalPrice = 99.98m,
                ProductImageUrl = "/images/wireless-mouse.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        modelBuilder.Entity<Order>().HasData(orders);
        modelBuilder.Entity<OrderItem>().HasData(orderItems);
    }
} 