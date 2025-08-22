using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries
{
    public class GetReviewByIdQuery : IRequest<ReviewDTO?>
    {
        public Guid ReviewId { get; set; }
    }
}