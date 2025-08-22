using MediatR;

namespace OrderService.Application.Commands
{
    public class DeleteReviewCommand : IRequest<bool>
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
    }
}