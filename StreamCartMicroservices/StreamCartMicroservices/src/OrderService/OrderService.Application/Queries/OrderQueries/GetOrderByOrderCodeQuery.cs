using MediatR;
using OrderService.Application.DTOs.OrderDTOs;

namespace OrderService.Application.Queries.OrderQueries
{
    public class GetOrderByOrderCodeQuery : IRequest<OrderDto>
    {
        public string OrderCode { get; set; } = string.Empty;
    }
}