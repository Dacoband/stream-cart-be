using System;
using MediatR;
using OrderService.Application.DTOs.OrderItemDTOs;

namespace OrderService.Application.Queries.OrderItemQueries
{
    /// <summary>
    /// Query to get an order item by its ID
    /// </summary>
    public class GetOrderItemByIdQuery : IRequest<OrderItemDto>
    {
        /// <summary>
        /// The unique identifier of the order item
        /// </summary>
        public Guid OrderItemId { get; set; }
    }
}