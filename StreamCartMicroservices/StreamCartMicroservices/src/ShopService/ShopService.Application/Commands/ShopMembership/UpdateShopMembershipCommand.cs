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
    public class UpdateShopMembershipCommand : IRequest<ApiResponse<DetailShopMembershipDTO>>
    {
        public string ShopId { get; set; }
        public int RemainingLivestream {  get; set; }

    }
}
