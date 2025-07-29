using MediatR;
using Shared.Common.Models;
using ShopService.Application.Commands;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.MembershipHandler
{
    public class CreateMembershipCommandHandler : IRequestHandler<CreateMembershipCommand, ApiResponse<DetailMembershipDTO>>
    {
        private readonly IMembershipService _membershipService;
        public CreateMembershipCommandHandler(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }
        public async Task<ApiResponse<DetailMembershipDTO>> Handle(CreateMembershipCommand request, CancellationToken cancellationToken)
        {
            return await _membershipService.CreateMembership(request.command, request.userId);
        }
    }
}
