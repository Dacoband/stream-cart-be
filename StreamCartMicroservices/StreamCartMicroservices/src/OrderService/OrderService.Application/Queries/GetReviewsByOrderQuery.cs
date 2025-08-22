using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries
{
    public class GetReviewsByOrderQuery : IRequest<IEnumerable<ReviewDTO>>
    {
        public Guid OrderId { get; set; }
    }
}