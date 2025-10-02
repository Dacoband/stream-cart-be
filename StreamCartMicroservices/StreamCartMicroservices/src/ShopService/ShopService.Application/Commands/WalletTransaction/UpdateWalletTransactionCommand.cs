using MediatR;
using Shared.Common.Models;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands.WalletTransaction
{
    public class UpdateWalletTransactionCommand : IRequest<ApiResponse<ShopService.Domain.Entities.WalletTransaction>>
    {
        public string WalletTransactionId { get; set; }
        public WalletTransactionStatus Status { get; set; }
        public string? ShopId { get; set; }
        public string UserId { get; set; }
        public string? PaymentTransactionId { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
