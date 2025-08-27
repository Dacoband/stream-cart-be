using MediatR;
using Shared.Common.Models;
using ShopService.Application.Commands.WalletTransaction;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.WalletTRansactionHandler
{
    public class CreateWalletTransactionHandler : IRequestHandler<CreateWalletTraansactionCommand, ApiResponse<ShopService.Domain.Entities.WalletTransaction>>
    {
        private readonly IWalletTransactionService _walletTransactionService;
        public CreateWalletTransactionHandler(IWalletTransactionService walletTransactionService)
        {
            _walletTransactionService = walletTransactionService;
        }
        public async Task<ApiResponse<WalletTransaction>> Handle(CreateWalletTraansactionCommand request, CancellationToken cancellationToken)
        {
            return await _walletTransactionService.CreateWalletTransaction(request.CreateWalletTransactionDTO, request.ShopId, request.UserId);
        }
    }
}
