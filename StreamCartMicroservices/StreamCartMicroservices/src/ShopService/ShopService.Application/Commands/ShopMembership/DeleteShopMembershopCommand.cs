
using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands.ShopMembership
{
    public class DeleteShopMembershipCommand : IRequest<ApiResponse<DetailShopMembershipDTO>>
    {
        public required string UserId { get; set; }
        public required string ShopMembershipId { get; set; }
    }
}
