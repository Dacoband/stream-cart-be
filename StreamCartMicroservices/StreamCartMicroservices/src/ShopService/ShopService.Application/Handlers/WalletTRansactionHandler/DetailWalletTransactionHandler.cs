using MediatR;
using Shared.Common.Models;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.WalletTransaction;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.WalletTRansactionHandler
{
    public class DetailWalletTransactionHandler : IRequestHandler<DetailWalletTransactionDTO, ApiResponse<WalletTransaction>>
    {
        private readonly IWalletTransactionService _walletTransactionService;
        public DetailWalletTransactionHandler(IWalletTransactionService walletTransactionService)
        {
            _walletTransactionService = walletTransactionService;
        }
        public async Task<ApiResponse<WalletTransaction>> Handle(DetailWalletTransactionDTO request, CancellationToken cancellationToken)
        {
            return await _walletTransactionService.GetWalletTransactionById(request.Id);
        }
    }
}
