using MediatR;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Queries.OrderQueries
{
    /// <summary>
    /// Query to get an order by its order code
    /// </summary>
    public class GetOrderByCodeQuery : IRequest<OrderDto>
    {
        /// <summary>
        /// The unique order code
        /// </summary>
        public string OrderCode { get; set; } = string.Empty;
    }
}