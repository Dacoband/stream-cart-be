using System;
using System.Collections.Generic;
using MediatR;
using OrderService.Application.DTOs.OrderItemDTOs;

namespace OrderService.Application.Queries.OrderItemQueries
{
    /// <summary>
    /// Query to get sales statistics for all products in a shop
    /// </summary>
    public class GetShopSalesStatisticsQuery : IRequest<IEnumerable<ProductSalesStatisticsDto>>
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
        
        /// <summary>
        /// Optional limit for the number of products to return (top selling products)
        /// </summary>
        public int? TopProductsLimit { get; set; }
    }
}