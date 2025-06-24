using System;
using System.Collections.Generic;
using MediatR;
using OrderService.Application.DTOs.OrderItemDTOs;

namespace OrderService.Application.Queries.OrderItemQueries
{
    /// <summary>
    /// Query to get all items for a specific order
    /// </summary>
    public class GetOrderItemsByOrderIdQuery : IRequest<IEnumerable<OrderItemDto>>
    {
        /// <summary>
        /// The order ID to get items for
        /// </summary>
        public Guid OrderId { get; set; }
    }
}