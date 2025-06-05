using AccountService.Application.DTOs;
using AccountService.Domain.Enums;
using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class UpdateAccountCommand : IRequest<AccountDto>
    {
        public Guid Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Fullname { get; set; }
        public string? AvatarURL { get; set; }
        public RoleType? Role { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsVerified { get; set; }
        public decimal? CompleteRate { get; set; }
        public Guid? ShopId { get; set; }
        public string? UpdatedBy { get; set; }
    }
}