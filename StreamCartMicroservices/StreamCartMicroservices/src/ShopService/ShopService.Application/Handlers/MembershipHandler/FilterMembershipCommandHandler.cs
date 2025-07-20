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

namespace ShopService.Application.Handlers.MembershipHandler
{
    public class FilterMembershipCommandHandler : IRequestHandler<FilterMembershipQuery, ApiResponse<ListMembershipDTO>>
    {
        private readonly IMembershipService _membershipService;
        public FilterMembershipCommandHandler(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }
        public async Task<ApiResponse<ListMembershipDTO>> Handle(FilterMembershipQuery request, CancellationToken cancellationToken)
        {
            return await _membershipService.FilterMembership(request.filter);
        }
    }
}
