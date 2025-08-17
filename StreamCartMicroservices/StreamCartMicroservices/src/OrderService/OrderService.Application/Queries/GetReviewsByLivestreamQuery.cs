using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries
{
    public class GetReviewsByLivestreamQuery : IRequest<IEnumerable<ReviewDTO>>
    {
        public Guid LivestreamId { get; set; }
    }
}