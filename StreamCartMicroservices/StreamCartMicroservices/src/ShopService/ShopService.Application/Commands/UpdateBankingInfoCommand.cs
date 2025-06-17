using MediatR;
using ShopService.Application.DTOs;
using System;

namespace ShopService.Application.Commands
{
    public class UpdateBankingInfoCommand : IRequest<ShopDto>
    {
        public Guid ShopId { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}