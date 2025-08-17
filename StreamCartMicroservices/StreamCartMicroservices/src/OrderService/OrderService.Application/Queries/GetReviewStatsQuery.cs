using MediatR;
using OrderService.Application.DTOs;
using ReviewService.Domain.Enums;

namespace OrderService.Application.Queries
{
    public class GetReviewStatsQuery : IRequest<ReviewStatsDTO>
    {
        public Guid TargetId { get; set; }
        public ReviewType Type { get; set; }
    }
}