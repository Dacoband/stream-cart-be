using MediatR;
using ShopService.Application.DTOs;
using System;

namespace ShopService.Application.Queries
{
    public class GetShopByIdQuery : IRequest<ShopDto>
    {
        public Guid Id { get; set; }
    }
}