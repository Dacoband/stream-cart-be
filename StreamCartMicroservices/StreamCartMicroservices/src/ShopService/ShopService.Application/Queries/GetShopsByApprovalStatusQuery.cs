using MediatR;
using ShopService.Application.DTOs;
using ShopService.Domain.Enums;
using System.Collections.Generic;

namespace ShopService.Application.Queries
{
    public class GetShopsByApprovalStatusQuery : IRequest<IEnumerable<ShopDto>>
    {
        public ApprovalStatus Status { get; set; }
    }
}