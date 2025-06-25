using System;
using MediatR;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Queries.OrderQueries
{
    /// <summary>
    /// Query to get order statistics for a shop
    /// </summary>
    public class GetOrderStatisticsQuery : IRequest<OrderStatisticsDto>
    {
        /// <summary>
        /// The shop ID to get statistics for
        /// </summary>
        public Guid ShopId { get; set; }
        
        /// <summary>
        /// Optional start date for filtering statistics
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Optional end date for filtering statistics
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}