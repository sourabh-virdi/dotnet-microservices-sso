using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Services;
using Shared.Common.DTOs;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders (Admin) or user's orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderSummaryDto>>>> GetOrders()
    {
        try
        {
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            
            var orders = await _orderService.GetOrdersAsync(isAdmin ? null : userId);
            
            return Ok(new ApiResponse<IEnumerable<OrderSummaryDto>>
            {
                Success = true,
                Data = orders,
                Message = "Orders retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching orders");
            return StatusCode(500, new ApiResponse<IEnumerable<OrderSummaryDto>>
            {
                Success = false,
                Message = "An error occurred while fetching orders"
            });
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
    {
        try
        {
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            
            var order = await _orderService.GetOrderByIdAsync(id, isAdmin ? null : userId);

            if (order == null)
            {
                return NotFound(new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Order not found"
                });
            }

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Data = order,
                Message = "Order retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching order {OrderId}", id);
            return StatusCode(500, new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "An error occurred while fetching the order"
            });
        }
    }

    /// <summary>
    /// Get current user's orders
    /// </summary>
    [HttpGet("my-orders")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderSummaryDto>>>> GetMyOrders()
    {
        try
        {
            var userId = GetUserId();
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            
            return Ok(new ApiResponse<IEnumerable<OrderSummaryDto>>
            {
                Success = true,
                Data = orders,
                Message = "Your orders retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user orders");
            return StatusCode(500, new ApiResponse<IEnumerable<OrderSummaryDto>>
            {
                Success = false,
                Message = "An error occurred while fetching your orders"
            });
        }
    }

    /// <summary>
    /// Get orders by status (Admin only)
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderSummaryDto>>>> GetOrdersByStatus(OrderStatus status)
    {
        try
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            
            return Ok(new ApiResponse<IEnumerable<OrderSummaryDto>>
            {
                Success = true,
                Data = orders,
                Message = $"Orders with status '{status}' retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching orders by status {Status}", status);
            return StatusCode(500, new ApiResponse<IEnumerable<OrderSummaryDto>>
            {
                Success = false,
                Message = "An error occurred while fetching orders"
            });
        }
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Invalid order data"
                });
            }

            var userId = GetUserId();
            var order = await _orderService.CreateOrderAsync(createOrderDto, userId);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, 
                new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Order created successfully"
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating order");
            return StatusCode(500, new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "An error occurred while creating the order"
            });
        }
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateStatusDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Invalid status update data"
                });
            }

            var order = await _orderService.UpdateOrderStatusAsync(id, updateStatusDto);

            if (order == null)
            {
                return NotFound(new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = "Order not found"
                });
            }

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Data = order,
                Message = "Order status updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating order status {OrderId}", id);
            return StatusCode(500, new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "An error occurred while updating the order status"
            });
        }
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> CancelOrder(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _orderService.CancelOrderAsync(id, userId);

            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found or cannot be cancelled"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Order cancelled successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling order {OrderId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while cancelling the order"
            });
        }
    }

    /// <summary>
    /// Get total revenue (Admin only)
    /// </summary>
    [HttpGet("analytics/revenue")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var revenue = await _orderService.GetTotalRevenueAsync(startDate, endDate);
            
            return Ok(new ApiResponse<decimal>
            {
                Success = true,
                Data = revenue,
                Message = "Total revenue calculated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating revenue");
            return StatusCode(500, new ApiResponse<decimal>
            {
                Success = false,
                Message = "An error occurred while calculating revenue"
            });
        }
    }

    /// <summary>
    /// Get total orders count (Admin only)
    /// </summary>
    [HttpGet("analytics/count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> GetTotalOrdersCount([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var count = await _orderService.GetTotalOrdersCountAsync(startDate, endDate);
            
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Data = count,
                Message = "Total orders count retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while counting orders");
            return StatusCode(500, new ApiResponse<int>
            {
                Success = false,
                Message = "An error occurred while counting orders"
            });
        }
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
               User.FindFirstValue("sub") ?? 
               throw new UnauthorizedAccessException("User ID not found in claims");
    }
} 