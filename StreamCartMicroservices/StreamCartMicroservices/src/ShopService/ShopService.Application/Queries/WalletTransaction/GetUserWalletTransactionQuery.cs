using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.WalletTransaction;

namespace ShopService.Application.Queries.WalletTransaction
{
    public class GetUserWalletTransactionQuery : IRequest<ApiResponse<ListWalletransationDTO>>
    {
        public Guid UserId { get; set; }
        public FilterWalletTransactionDTO Filter { get; set; } = new();
    }
}