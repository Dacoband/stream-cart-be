using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.WalletTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries.WalletTransaction
{
    public class FilterWalletTransactionQuery : IRequest<ApiResponse<ListWalletransationDTO>>
    {
        public FilterWalletTransactionDTO Filter {  get; set; }
        public string? ShopId { get; set; }
    }
}
