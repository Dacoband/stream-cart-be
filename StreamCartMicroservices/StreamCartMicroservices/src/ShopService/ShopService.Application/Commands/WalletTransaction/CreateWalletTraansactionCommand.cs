using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.WalletTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands.WalletTransaction
{
    public class CreateWalletTraansactionCommand : IRequest<ApiResponse<ShopService.Domain.Entities.WalletTransaction>>
    {
        public CreateWalletTransactionDTO CreateWalletTransactionDTO { get; set; }
        public string? ShopId { get; set; }
        public string UserId { get; set; }
    }
}
