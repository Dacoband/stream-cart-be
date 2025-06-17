using MediatR;
using ShopService.Application.DTOs;
using System;

namespace ShopService.Application.Commands
{
    public class ApproveShopCommand : IRequest<ShopDto>
    {
        public Guid ShopId { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
    }
}