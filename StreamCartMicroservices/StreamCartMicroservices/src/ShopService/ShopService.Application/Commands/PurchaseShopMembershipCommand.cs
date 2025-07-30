using MediatR;
using Shared.Common.Models;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands
{
    public class PurchaseShopMembershipCommand : IRequest<ApiResponse<ShopService.Domain.Entities.ShopMembership>>
    {
        public string UserId { get; set; }
        public string MembershipId { get; set; }
    }
}
