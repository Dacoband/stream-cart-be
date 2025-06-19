using MediatR;
using ShopService.Application.DTOs;
using System;

namespace ShopService.Application.Commands
{
    public class RejectShopCommand : IRequest<ShopDto>
    {
        public Guid ShopId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public string RejectedBy { get; set; } = string.Empty;
    }
}