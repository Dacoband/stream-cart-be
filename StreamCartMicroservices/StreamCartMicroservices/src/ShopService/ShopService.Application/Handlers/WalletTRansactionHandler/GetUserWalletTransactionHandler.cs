using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.WalletTransaction;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.WalletTransaction;

namespace ShopService.Application.Handlers.WalletTransactionHandlers
{
    public class GetUserWalletTransactionHandler : IRequestHandler<GetUserWalletTransactionQuery, ApiResponse<ListWalletransationDTO>>
    {
        private readonly IWalletTransactionService _walletTransactionService;

        public GetUserWalletTransactionHandler(IWalletTransactionService walletTransactionService)
        {
            _walletTransactionService = walletTransactionService;
        }

        public async Task<ApiResponse<ListWalletransationDTO>> Handle(GetUserWalletTransactionQuery request, CancellationToken cancellationToken)
        {
            return await _walletTransactionService.GetUserWalletTransactionList(request.Filter, request.UserId);
        }
    }
}