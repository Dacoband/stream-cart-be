using Shared.Common.Models;
using ShopService.Application.DTOs.WalletTransaction;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IWalletTransactionService
    {
        public Task<ApiResponse<WalletTransaction>> CreateWalletTransaction(CreateWalletTransactionDTO request, string? shopId, string userId);
        public Task<ApiResponse<WalletTransaction>> GetWalletTransactionById(string id);
        public Task<ApiResponse<ListWalletransationDTO>> GetWalletTransactionList(FilterWalletTransactionDTO filterWalletTransactionDTO, string? shopId);
        public Task<ApiResponse<WalletTransaction>> UpdateWalletTransactionStatus(string id, WalletTransactionStatus status, string? shopId, string userid);
    }
}
