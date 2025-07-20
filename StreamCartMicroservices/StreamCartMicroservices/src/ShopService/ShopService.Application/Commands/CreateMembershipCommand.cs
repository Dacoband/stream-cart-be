using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;

namespace ShopService.Application.Commands
{
    public class CreateMembershipCommand : IRequest<ApiResponse<DetailMembershipDTO>>
    {
        public required CreateMembershipDTO command { get; set; }
        public required string userId { get; set; }
    }
}
