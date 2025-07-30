using MediatR;
using Shared.Common.Models;
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
    public class FilterShopMembershipHandler : IRequestHandler<FilterShopMembershipQuery, ApiResponse<ListShopMembershipDTO>>
    {
        private readonly IShopMembershipService _shopMembershipService;
        public FilterShopMembershipHandler(IShopMembershipService shopMembershipService)
        {
            _shopMembershipService = shopMembershipService;
        }
        public async Task<ApiResponse<ListShopMembershipDTO>> Handle(FilterShopMembershipQuery request, CancellationToken cancellationToken)
        {
            return await _shopMembershipService.FilterShopMembership(request.Filter);
        }
    }
}
