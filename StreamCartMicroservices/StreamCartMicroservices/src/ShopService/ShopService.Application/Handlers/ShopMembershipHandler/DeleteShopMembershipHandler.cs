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
    public class DeleteShopMembershipHandler : IRequestHandler<DeleteShopMembershipCommand, ApiResponse<DetailShopMembershipDTO>>
    {
        private readonly IShopMembershipService _shopMembershipService;
        public DeleteShopMembershipHandler(IShopMembershipService shopMembershipService)
        {
            _shopMembershipService = shopMembershipService;
        }
        public async Task<ApiResponse<DetailShopMembershipDTO>> Handle(DeleteShopMembershipCommand request, CancellationToken cancellationToken)
        {
            return await _shopMembershipService.DeleteShopMembership(request.ShopMembershipId, request.UserId);
        }
    }
}
