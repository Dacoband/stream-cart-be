using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;

namespace ShopService.Application.Commands
{
    public class UpdateMembershipCommand : IRequest<ApiResponse<DetailMembershipDTO>>
    {
        public required string UserId { get; set; }
        public required string MembershipId { get; set; }
        public required UpdateMembershipDTO command { get; set; }
    }
}
