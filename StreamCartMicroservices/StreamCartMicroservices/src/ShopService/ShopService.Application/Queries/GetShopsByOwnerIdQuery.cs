using MediatR;
using ShopService.Application.DTOs;
using System;
using System.Collections.Generic;

namespace ShopService.Application.Queries
{
    public class GetShopsByOwnerIdQuery : IRequest<IEnumerable<ShopDto>>
    {
        public Guid AccountId { get; set; }
    }
}