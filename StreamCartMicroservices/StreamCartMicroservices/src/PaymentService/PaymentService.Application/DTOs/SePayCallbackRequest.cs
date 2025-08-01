using System;

namespace PaymentService.Application.DTOs
{
    public class SePayCallbackRequest
    {
        public string? Gateway { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? SubAccount { get; set; }
        public string? Code { get; set; }
        public string? Content { get; set; }
        public string? TransferType { get; set; }
        public string? Description { get; set; }
        public decimal TransferAmount { get; set; }
        public string? ReferenceCode { get; set; }
        public decimal Accumulated { get; set; }
        public long Id { get; set; }

        // Properties để tương thích với code cũ
        public string? TransactionId => ReferenceCode;
        public string? OrderCode
        {
            get
            {
                if (string.IsNullOrEmpty(Content))
                    return null;

                // Tìm pattern ORDER_ hoặc ORDERS_ trong content
                var orderIndex = Content.IndexOf("ORDER", StringComparison.OrdinalIgnoreCase);
                if (orderIndex == -1)
                    return null;

                // Tìm phần sau ORDER hoặc ORDERS
                var remainingContent = Content.Substring(orderIndex);

                // Tách các phần bằng dấu chấm hoặc khoảng trắng
                var parts = remainingContent.Split(new char[] { '.', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    if (part.StartsWith("ORDER", StringComparison.OrdinalIgnoreCase))
                    {
                        return part;
                    }
                }

                return null;
            }
        }
        public decimal Amount => TransferAmount;
        public string? Status => TransferType == "in" ? "success" : "failed";
    }
}