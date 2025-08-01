using MediatR;
using OrderService.Application.DTOs.OrderDTOs;
using Shared.Common.Models;
using System;
using System.Collections.Generic;

namespace OrderService.Application.Commands.OrderCommands
{
    public class CreateMultiOrderCommand : IRequest<ApiResponse<List<OrderDto>>>
    {
        public Guid AccountId { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string AddressId { get; set; }
        public Guid? LivestreamId { get; set; }
        public Guid? CreatedFromCommentId { get; set; }
        public List<CreateOrderByShopDto> OrdersByShop { get; set; } = new();
    }
}