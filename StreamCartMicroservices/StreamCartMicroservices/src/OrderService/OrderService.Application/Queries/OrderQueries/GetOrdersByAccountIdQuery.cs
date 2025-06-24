using System;
using MediatR;
using Shared.Common.Domain.Bases;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Queries.OrderQueries
{
    /// <summary>
    /// Query to get orders for a specific account with pagination
    /// </summary>
    public class GetOrdersByAccountIdQuery : IRequest<PagedResult<OrderDto>>
    {
        /// <summary>
        /// The account ID to get orders for
        /// </summary>
        public Guid AccountId { get; set; }
        
        /// <summary>
        /// Page number for pagination (starts at 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Items per page for pagination
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}