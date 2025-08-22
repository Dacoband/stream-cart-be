using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries
{
    public class GetReviewsByUserQuery : IRequest<IEnumerable<ReviewDTO>>
    {
        public Guid UserId { get; set; }
    }
}