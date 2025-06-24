using MediatR;
using Shared.Common.Domain.Bases;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Queries.OrderQueries
{
    /// <summary>
    /// Query to search for orders with various filters
    /// </summary>
    public class SearchOrdersQuery : IRequest<PagedResult<OrderDto>>
    {
        /// <summary>
        /// Search parameters for filtering orders
        /// </summary>
        public OrderSearchParamsDto SearchParams { get; set; } = new OrderSearchParamsDto();
    }
}