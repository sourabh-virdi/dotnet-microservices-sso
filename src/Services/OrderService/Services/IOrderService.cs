using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderSummaryDto>> GetOrdersAsync(string? userId = null);
    Task<OrderDto?> GetOrderByIdAsync(int id, string? userId = null);
    Task<IEnumerable<OrderSummaryDto>> GetOrdersByUserAsync(string userId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userId);
    Task<OrderDto?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto updateStatusDto);
    Task<bool> CancelOrderAsync(int id, string userId);
    Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetTotalOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null);
} 