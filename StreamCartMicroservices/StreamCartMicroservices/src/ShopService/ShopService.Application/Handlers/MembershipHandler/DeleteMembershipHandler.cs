using MediatR;
using Shared.Common.Models;
using ShopService.Application.Commands;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.MembershipHandler
{
    public class DeleteMembershipHandler : IRequestHandler<DeleteMemershipCommand, ApiResponse<DetailMembershipDTO>>
    {
        private readonly IMembershipService _membershipService;
        public DeleteMembershipHandler(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }
        public async Task<ApiResponse<DetailMembershipDTO>> Handle(DeleteMemershipCommand request, CancellationToken cancellationToken)
        {
            return await _membershipService.DeleteMembership(request.MembershipId, request.UserId);
        }
    }
}
