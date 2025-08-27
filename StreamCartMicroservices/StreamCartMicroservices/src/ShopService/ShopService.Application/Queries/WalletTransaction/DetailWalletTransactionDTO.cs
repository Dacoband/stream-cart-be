using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries.WalletTransaction
{
    public class DetailWalletTransactionDTO : IRequest<ApiResponse<ShopService.Domain.Entities.WalletTransaction>>
    {
        public string Id { get; set; }
    }
}
