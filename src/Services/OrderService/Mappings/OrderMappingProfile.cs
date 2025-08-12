using AutoMapper;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Order mappings
        CreateMap<Order, OrderDto>();
        CreateMap<Order, OrderSummaryDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems.Count));

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
            .ForMember(dest => dest.ShippingCost, opt => opt.Ignore())
            .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentReference, opt => opt.Ignore());

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>();
        
        CreateMap<CreateOrderItemDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore());
    }
} 