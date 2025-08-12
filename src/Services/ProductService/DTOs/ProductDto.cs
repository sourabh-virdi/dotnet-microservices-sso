using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class ProductDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsAvailable { get; set; } = true;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    public DateTime? LaunchDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProductDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsAvailable { get; set; } = true;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    public DateTime? LaunchDate { get; set; }
}

public class UpdateProductDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsAvailable { get; set; }

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    public DateTime? LaunchDate { get; set; }
} 