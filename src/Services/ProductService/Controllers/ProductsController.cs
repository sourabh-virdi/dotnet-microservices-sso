using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;
using Shared.Common.DTOs;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = true,
                Data = products,
                Message = "Products retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching products");
            return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = false,
                Message = "An error occurred while fetching products"
            });
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            return Ok(new ApiResponse<ProductDto>
            {
                Success = true,
                Data = product,
                Message = "Product retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching product {ProductId}", id);
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "An error occurred while fetching the product"
            });
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsByCategory(string category)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(category);
            return Ok(new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = true,
                Data = products,
                Message = $"Products in category '{category}' retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching products for category {Category}", category);
            return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = false,
                Message = "An error occurred while fetching products"
            });
        }
    }

    /// <summary>
    /// Search products
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> SearchProducts([FromQuery] string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "Search term is required"
                });
            }

            var products = await _productService.SearchProductsAsync(term);
            return Ok(new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = true,
                Data = products,
                Message = $"Search results for '{term}' retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching products with term {SearchTerm}", term);
            return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
            {
                Success = false,
                Message = "An error occurred while searching products"
            });
        }
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetCategories()
    {
        try
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(new ApiResponse<IEnumerable<string>>
            {
                Success = true,
                Data = categories,
                Message = "Categories retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching categories");
            return StatusCode(500, new ApiResponse<IEnumerable<string>>
            {
                Success = false,
                Message = "An error occurred while fetching categories"
            });
        }
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Invalid product data"
                });
            }

            var product = await _productService.CreateProductAsync(createProductDto);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Product created successfully"
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "An error occurred while creating the product"
            });
        }
    }

    /// <summary>
    /// Update a product (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Invalid product data"
                });
            }

            var product = await _productService.UpdateProductAsync(id, updateProductDto);

            if (product == null)
            {
                return NotFound(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            return Ok(new ApiResponse<ProductDto>
            {
                Success = true,
                Data = product,
                Message = "Product updated successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<ProductDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product {ProductId}", id);
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "An error occurred while updating the product"
            });
        }
    }

    /// <summary>
    /// Delete a product (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product {ProductId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the product"
            });
        }
    }

    /// <summary>
    /// Update product stock (Admin only)
    /// </summary>
    [HttpPut("{id}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStock(int id, [FromBody] int quantity)
    {
        try
        {
            if (quantity < 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Quantity cannot be negative"
                });
            }

            var result = await _productService.UpdateStockAsync(id, quantity);

            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Stock updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating stock for product {ProductId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating stock"
            });
        }
    }
} 