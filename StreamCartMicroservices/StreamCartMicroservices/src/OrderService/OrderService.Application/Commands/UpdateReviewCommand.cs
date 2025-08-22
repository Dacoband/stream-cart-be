using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Commands
{
    public class UpdateReviewCommand : IRequest<ReviewDTO>
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public int Rating { get; set; }
        public List<string>? ImageUrls { get; set; } = new List<string>();
    }
}