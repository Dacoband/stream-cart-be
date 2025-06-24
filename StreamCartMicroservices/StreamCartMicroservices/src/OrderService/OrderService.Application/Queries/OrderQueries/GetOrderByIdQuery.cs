using System;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Queries.OrderQueries
{
    /// <summary>
    /// Query to get an order by its ID
    /// </summary>
    public class GetOrderByIdQuery : IRequest<OrderDto>
    {
        /// <summary>
        /// The unique identifier of the order
        /// </summary>
        public Guid OrderId { get; set; }
    }
}