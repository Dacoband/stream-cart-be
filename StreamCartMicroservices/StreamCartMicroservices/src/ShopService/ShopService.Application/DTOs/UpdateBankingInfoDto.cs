using System;

namespace ShopService.Application.DTOs
{
    public class UpdateBankingInfoDto
    {
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
    }
}