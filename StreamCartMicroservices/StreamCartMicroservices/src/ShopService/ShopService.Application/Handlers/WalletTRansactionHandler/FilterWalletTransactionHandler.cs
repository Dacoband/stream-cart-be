using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.WalletTransaction;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.WalletTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.WalletTRansactionHandler
{
    public class FilterWalletTransactionHandler : IRequestHandler<FilterWalletTransactionQuery, ApiResponse<ListWalletransationDTO>>
    {
        private readonly IWalletTransactionService _walletTransactionService;
        public FilterWalletTransactionHandler(IWalletTransactionService walletTransactionService)
        {
            _walletTransactionService = walletTransactionService;
        }
        public async Task<ApiResponse<ListWalletransationDTO>> Handle(FilterWalletTransactionQuery request, CancellationToken cancellationToken)
        {
            return await _walletTransactionService.GetWalletTransactionList(request.Filter, request.ShopId);
        }
    }
}
