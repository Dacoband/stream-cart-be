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
    public class GetDetailMembershipHandler : IRequestHandler<GetMembershipByIdQuery, ApiResponse<DetailMembershipDTO>>
    {
        private readonly IMembershipService _membershipService;
        public GetDetailMembershipHandler(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }
        public async Task<ApiResponse<DetailMembershipDTO>> Handle(GetMembershipByIdQuery request, CancellationToken cancellationToken)
        {
            return await _membershipService.GetMembershipById(request.MembershipId);
        }
    }
}
