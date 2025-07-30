
using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries
{
    public class DetailShopMembershipQuery : IRequest<ApiResponse<DetailShopMembershipDTO>>
    {
        public required string ShopMembershipId { get; set; }
    }
}
