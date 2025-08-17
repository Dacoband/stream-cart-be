using MediatR;
using OrderService.Application.DTOs;
using ReviewService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Commands
{
    public class CreateReviewCommand : IRequest<ReviewDTO>
    {
        public Guid? OrderID { get; set; }
        public Guid? ProductID { get; set; }
        public Guid? LivestreamId { get; set; }
        public Guid AccountID { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public ReviewType Type { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsVerifiedPurchase { get; set; }
    }
}
