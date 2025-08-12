using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Services;

public class OrderServiceImpl : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderServiceImpl> _logger;
    private const decimal TaxRate = 0.08m; // 8% tax
    private const decimal ShippingCost = 9.99m;

    public OrderServiceImpl(OrderDbContext context, IMapper mapper, ILogger<OrderServiceImpl> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersAsync(string? userId = null)
    {
        _logger.LogInformation("Fetching orders. UserId filter: {UserId}", userId ?? "None");

        var query = _context.Orders.Include(o => o.OrderItems).AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(o => o.UserId == userId);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id, string? userId = null)
    {
        _logger.LogInformation("Fetching order with ID {OrderId}. UserId filter: {UserId}", id, userId ?? "None");

        var query = _context.Orders
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(o => o.UserId == userId);
        }

        var order = await query.FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", id);
            return null;
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByUserAsync(string userId)
    {
        _logger.LogInformation("Fetching orders for user {UserId}", userId);

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userId)
    {
        _logger.LogInformation("Creating new order for user {UserId}", userId);

        if (!createOrderDto.OrderItems.Any())
        {
            throw new ArgumentException("Order must contain at least one item");
        }

        var order = _mapper.Map<Order>(createOrderDto);
        order.UserId = userId;
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        order.PaymentReference = GeneratePaymentReference();

        // Calculate totals
        var orderItems = _mapper.Map<List<OrderItem>>(createOrderDto.OrderItems);
        foreach (var item in orderItems)
        {
            item.TotalPrice = item.Quantity * item.UnitPrice;
            item.CreatedAt = DateTime.UtcNow;
        }

        order.OrderItems = orderItems;
        order.SubTotal = orderItems.Sum(i => i.TotalPrice);
        order.ShippingCost = ShippingCost;
        order.TaxAmount = order.SubTotal * TaxRate;
        order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingCost;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order created with ID {OrderId}", order.Id);

        // Reload with includes for proper mapping
        var createdOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.Id == order.Id);

        return _mapper.Map<OrderDto>(createdOrder);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto updateStatusDto)
    {
        _logger.LogInformation("Updating order status for ID {OrderId} to {Status}", id, updateStatusDto.Status);

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", id);
            return null;
        }

        order.Status = updateStatusDto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(updateStatusDto.TrackingNumber))
        {
            order.TrackingNumber = updateStatusDto.TrackingNumber;
        }

        if (!string.IsNullOrEmpty(updateStatusDto.Notes))
        {
            order.Notes = updateStatusDto.Notes;
        }

        // Set dates based on status
        switch (updateStatusDto.Status)
        {
            case OrderStatus.Shipped:
                order.ShippedDate = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.DeliveredDate = DateTime.UtcNow;
                if (!order.ShippedDate.HasValue)
                {
                    order.ShippedDate = DateTime.UtcNow.AddDays(-1); // Assume shipped yesterday
                }
                break;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order status updated for ID {OrderId}", order.Id);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<bool> CancelOrderAsync(int id, string userId)
    {
        _logger.LogInformation("Cancelling order {OrderId} for user {UserId}", id, userId);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found for user {UserId}", id, userId);
            return false;
        }

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            _logger.LogWarning("Cannot cancel order {OrderId} with status {Status}", id, order.Status);
            throw new InvalidOperationException("Cannot cancel an order that has been shipped or delivered");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order cancelled with ID {OrderId}", order.Id);

        return true;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status)
    {
        _logger.LogInformation("Fetching orders with status {Status}", status);

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogInformation("Calculating total revenue from {StartDate} to {EndDate}", 
            startDate?.ToString() ?? "beginning", endDate?.ToString() ?? "now");

        var query = _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Shipped);

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endDate.Value);
        }

        var revenue = await query.SumAsync(o => o.TotalAmount);

        _logger.LogInformation("Total revenue calculated: {Revenue}", revenue);

        return revenue;
    }

    public async Task<int> GetTotalOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogInformation("Counting total orders from {StartDate} to {EndDate}", 
            startDate?.ToString() ?? "beginning", endDate?.ToString() ?? "now");

        var query = _context.Orders.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endDate.Value);
        }

        var count = await query.CountAsync();

        _logger.LogInformation("Total orders count: {Count}", count);

        return count;
    }

    private static string GeneratePaymentReference()
    {
        return $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
} 