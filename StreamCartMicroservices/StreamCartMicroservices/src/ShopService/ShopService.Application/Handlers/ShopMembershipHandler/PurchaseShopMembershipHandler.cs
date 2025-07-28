using MediatR;
using Shared.Common.Models;
using ShopService.Application.Commands;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.ShopMembershipHandler
{
    public class PurchaseShopMembershipHandler : IRequestHandler<PurchaseShopMembershipCommand, ApiResponse<ShopMembership>>
    {
        private readonly IShopMembershipService _shopMembershipService;
        public PurchaseShopMembershipHandler(IShopMembershipService shopMembershipService)
        {
            _shopMembershipService = shopMembershipService;
        }
        public async Task<ApiResponse<ShopMembership>> Handle(PurchaseShopMembershipCommand request, CancellationToken cancellationToken)
        {
            return await _shopMembershipService.CreateShopMembership(request.MembershipId, request.UserId);
        }
    }
}
