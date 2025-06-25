using System;
using MediatR;
using OrderService.Application.DTOs.OrderItemDTOs;

namespace OrderService.Application.Queries.OrderItemQueries
{
    /// <summary>
    /// Query to get sales statistics for a product
    /// </summary>
    public class GetProductSalesStatisticsQuery : IRequest<ProductSalesStatisticsDto>
    {
        /// <summary>
        /// The product ID to get statistics for
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Optional shop ID to filter statistics by shop
        /// </summary>
        public Guid? ShopId { get; set; }
        
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