using MediatR;
using Shared.Common.Models;
using ShopService.Application.Commands.ShopMembership;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.ShopMembershipHandler
{
    public class DetailShopMembershipHandler : IRequestHandler<DetailShopMembershipQuery, ApiResponse<DetailShopMembershipDTO>>
    {
        private readonly IShopMembershipService _shopMembershipService;
        public DetailShopMembershipHandler(IShopMembershipService shopMembershipService)
        {
            _shopMembershipService = shopMembershipService;
        }
        public async Task<ApiResponse<DetailShopMembershipDTO>> Handle(DetailShopMembershipQuery request, CancellationToken cancellationToken)
        {
            return await _shopMembershipService.GetShopMembershipById(request.ShopMembershipId);
        }
    }
}
