using MediatR;
using ShopService.Application.DTOs;
using System;

namespace ShopService.Application.Commands
{
    public class UpdateShopCommand : IRequest<ShopDto>
    {
        public Guid Id { get; set; }
        public string? ShopName { get; set; }
        public string? Description { get; set; }
        public string? LogoURL { get; set; }
        public string? CoverImageURL { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}