using MediatR;
using Shared.Common.Models;
using ShopService.Application.Commands.ShopMembership;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.ShopMembershipHandler
{
    public class UpdateShopMembershipHandler : IRequestHandler<UpdateShopMembershipCommand, ApiResponse<DetailShopMembershipDTO>>
    {
        private readonly IShopMembershipService _shopMembershipService;
        public UpdateShopMembershipHandler(IShopMembershipService shopMembershipService)
        {
            _shopMembershipService = shopMembershipService;
        }
        public async Task<ApiResponse<DetailShopMembershipDTO>> Handle(UpdateShopMembershipCommand request, CancellationToken cancellationToken)
        {
            return await _shopMembershipService.UpdateShopMembership(request.ShopId, request.RemainingLivestream);
        }
    }
}
