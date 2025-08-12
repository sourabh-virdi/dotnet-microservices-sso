using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services;

public class ProductServiceImpl : IProductService
{
    private readonly ProductDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductServiceImpl> _logger;

    public ProductServiceImpl(ProductDbContext context, IMapper mapper, ILogger<ProductServiceImpl> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        _logger.LogInformation("Fetching all available products");

        var products = await _context.Products
            .Where(p => p.IsAvailable)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        _logger.LogInformation("Fetching product with ID {ProductId}", id);

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsAvailable);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found or not available", id);
            return null;
        }

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
    {
        _logger.LogInformation("Fetching products in category {Category}", category);

        var products = await _context.Products
            .Where(p => p.Category.ToLower() == category.ToLower() && p.IsAvailable)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        _logger.LogInformation("Searching products with term: {SearchTerm}", searchTerm);

        var products = await _context.Products
            .Where(p => p.IsAvailable && 
                       (p.Name.Contains(searchTerm) || 
                        p.Description.Contains(searchTerm) ||
                        p.Brand.Contains(searchTerm) ||
                        p.Category.Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", createProductDto.Name);

        // Check if SKU already exists
        if (!string.IsNullOrEmpty(createProductDto.SKU))
        {
            var existingSku = await _context.Products
                .AnyAsync(p => p.SKU == createProductDto.SKU);

            if (existingSku)
            {
                _logger.LogError("Product with SKU {SKU} already exists", createProductDto.SKU);
                throw new InvalidOperationException($"Product with SKU '{createProductDto.SKU}' already exists.");
            }
        }

        var product = _mapper.Map<Product>(createProductDto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product created with ID {ProductId}", product.Id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        _logger.LogInformation("Updating product with ID {ProductId}", id);

        var product = await _context.Products.FindAsync(id);

        if (product == null || !product.IsAvailable)
        {
            _logger.LogWarning("Product with ID {ProductId} not found or not available", id);
            return null;
        }

        // Check if SKU already exists (excluding current product)
        if (!string.IsNullOrEmpty(updateProductDto.SKU))
        {
            var existingSku = await _context.Products
                .AnyAsync(p => p.SKU == updateProductDto.SKU && p.Id != id);

            if (existingSku)
            {
                _logger.LogError("Product with SKU {SKU} already exists", updateProductDto.SKU);
                throw new InvalidOperationException($"Product with SKU '{updateProductDto.SKU}' already exists.");
            }
        }

        _mapper.Map(updateProductDto, product);
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product updated with ID {ProductId}", product.Id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        _logger.LogInformation("Deleting product with ID {ProductId}", id);

        var product = await _context.Products.FindAsync(id);

        if (product == null || !product.IsAvailable)
        {
            _logger.LogWarning("Product with ID {ProductId} not found or already deleted", id);
            return false;
        }

        // Soft delete
        product.IsAvailable = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product soft deleted with ID {ProductId}", product.Id);

        return true;
    }

    public async Task<bool> UpdateStockAsync(int id, int quantity)
    {
        _logger.LogInformation("Updating stock for product ID {ProductId} to {Quantity}", id, quantity);

        var product = await _context.Products.FindAsync(id);

        if (product == null || !product.IsAvailable)
        {
            _logger.LogWarning("Product with ID {ProductId} not found or not available", id);
            return false;
        }

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Stock updated for product ID {ProductId}", product.Id);

        return true;
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        _logger.LogInformation("Fetching all product categories");

        var categories = await _context.Products
            .Where(p => p.IsAvailable)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return categories;
    }
} 