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
    public class UpdateWalletTransaction : IRequestHandler<UpdateWalletTransactionCommand, ApiResponse<ShopService.Domain.Entities.WalletTransaction>>
    {
        private readonly IWalletTransactionService _walletTransactionService;
        public UpdateWalletTransaction(IWalletTransactionService walletTransactionService)
        {
            _walletTransactionService = walletTransactionService;
        }
        public async Task<ApiResponse<WalletTransaction>> Handle(UpdateWalletTransactionCommand request, CancellationToken cancellationToken)
        {
            return await _walletTransactionService.UpdateWalletTransactionStatus(request.WalletTransactionId, request.Status, request.ShopId, request.UserId);
        }
    }
}
