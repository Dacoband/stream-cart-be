using MediatR;
using AccountService.Application.DTOs;
using System;
using System.Collections.Generic;

namespace AccountService.Application.Queries
{
    public class GetAccountsByShopIdQuery : IRequest<IEnumerable<AccountDto>>
    {
        public Guid ShopId { get; set; }
    }
}