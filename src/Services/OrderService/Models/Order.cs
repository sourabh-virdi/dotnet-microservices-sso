using Shared.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class Order : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [StringLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(500)]
    public string BillingAddress { get; set; } = string.Empty;

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public decimal TaxAmount { get; set; }

    [Required]
    public decimal ShippingCost { get; set; }

    [Required]
    public decimal SubTotal { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [StringLength(100)]
    public string PaymentReference { get; set; } = string.Empty;

    public DateTime? ShippedDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    [StringLength(50)]
    public string TrackingNumber { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
} 