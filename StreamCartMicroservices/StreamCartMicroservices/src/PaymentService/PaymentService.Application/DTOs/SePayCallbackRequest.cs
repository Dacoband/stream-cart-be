//using System;

//namespace PaymentService.Application.DTOs
//{
//    public class SePayCallbackRequest
//    {
//        public string? Gateway { get; set; }
//        public string? TransactionDate { get; set; }
//        public string? AccountNumber { get; set; }
//        public string? SubAccount { get; set; }
//        public string? Code { get; set; }
//        public string? Content { get; set; }
//        public string? TransferType { get; set; }
//        public string? Description { get; set; }
//        public decimal TransferAmount { get; set; }
//        public string? ReferenceCode { get; set; }
//        public decimal Accumulated { get; set; }
//        public long Id { get; set; }

//        // Properties để tương thích với code cũ
//        public string? TransactionId => ReferenceCode;
//        public string? OrderCode
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(Content))
//                    return null;

//                // Tìm pattern ORDER_ hoặc ORDERS_ trong content
//                var orderIndex = Content.IndexOf("ORDER", StringComparison.OrdinalIgnoreCase);
//                if (orderIndex == -1)
//                    return null;

//                // Tìm phần sau ORDER hoặc ORDERS
//                var remainingContent = Content.Substring(orderIndex);

//                var nextDash = remainingContent.IndexOf('-', 5); 

//                if (nextDash > 5) // Có dấu gạch ngang phía sau
//                {
//                    return remainingContent.Substring(0, nextDash);
//                }

//                // Nếu không có dấu gạch ngang, lấy toàn bộ
//                return remainingContent;
//            }
//        }
//        public decimal Amount => TransferAmount;
//        public string? Status => TransferType == "in" ? "success" : "failed";
//    }
//}


using System;
using System.Text.RegularExpressions;

namespace PaymentService.Application.DTOs
{
    public class SePayCallbackRequest
    {
        #region SePay Raw Properties
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
        #endregion

        #region Computed Properties for Backward Compatibility
        public string? TransactionId => ReferenceCode;
        public string? OrderCode => ExtractOrderCodeFromContent();
        public decimal Amount => TransferAmount;
        public string Status => "success";
        #endregion

        #region Private Helper Methods
        private string? ExtractOrderCodeFromContent()
        {
            if (string.IsNullOrEmpty(Content))
                return null;
            var patterns = new[]
            {
                @"WITHDRAW_CONFIRM_[0-9a-fA-F]{32}",          
                @"ORDERS_[0-9a-fA-F,]{32,}",                 
                @"DEPOSIT_[0-9a-fA-F]{32}",                   
                @"WITHDRAW_[0-9a-fA-F]{32}",                  
                @"ORDER_[0-9a-fA-F]{32}",                     
                @"ORDER[0-9a-fA-F]{32}",                      
                
                @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",        
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(Content, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var result = match.Value;
                    if (result.StartsWith("ORDERS_", StringComparison.OrdinalIgnoreCase))
                    {
                        return CleanOrdersPattern(result);
                    }

                    return result;
                }
            }
            return ExtractFallbackGuid();
        }
        private string CleanOrdersPattern(string ordersMatch)
        {
            try
            {
                var orderIdsString = ordersMatch.Substring(7); 
                var orderIds = orderIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var cleanedIds = new List<string>();
                foreach (var id in orderIds)
                {
                    var trimmedId = id.Trim();
                    if (trimmedId.Length == 32 && Regex.IsMatch(trimmedId, @"^[0-9a-fA-F]{32}$"))
                    {
                        cleanedIds.Add(trimmedId);
                    }
                    else if (Guid.TryParse(trimmedId, out _))
                    {
                        cleanedIds.Add(trimmedId.Replace("-", ""));
                    }
                }

                return cleanedIds.Count > 0 ? $"ORDERS_{string.Join(",", cleanedIds)}" : ordersMatch;
            }
            catch
            {
                return ordersMatch; 
            }
        }
        private string? ExtractFallbackGuid()
        {
            if (string.IsNullOrEmpty(Content))
                return null;

            // Look for any sequence that might be a GUID
            var guidMatches = Regex.Matches(Content, @"[0-9a-fA-F]{32}|[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.IgnoreCase);

            if (guidMatches.Count > 0)
            {
                // Return the first valid GUID found
                return guidMatches[0].Value;
            }

            return null;
        }
        #endregion

        #region Helper Properties for Debugging
        /// <summary>
        /// Determines the callback type based on OrderCode pattern
        /// Useful for debugging and logging
        /// </summary>
        public string CallbackType
        {
            get
            {
                var orderCode = OrderCode;
                if (string.IsNullOrEmpty(orderCode))
                    return "UNKNOWN";

                if (orderCode.StartsWith("ORDERS_", StringComparison.OrdinalIgnoreCase))
                    return "BULK_ORDER";
                if (orderCode.StartsWith("ORDER", StringComparison.OrdinalIgnoreCase))
                    return "SINGLE_ORDER";
                if (orderCode.StartsWith("DEPOSIT_", StringComparison.OrdinalIgnoreCase))
                    return "DEPOSIT";
                if (orderCode.StartsWith("WITHDRAW_CONFIRM_", StringComparison.OrdinalIgnoreCase))
                    return "WITHDRAWAL_CONFIRMATION";
                if (orderCode.StartsWith("WITHDRAW_", StringComparison.OrdinalIgnoreCase))
                    return "WITHDRAWAL";

                return "RAW_GUID";
            }
        }

        /// <summary>
        /// Indicates the direction of money flow
        /// </summary>
        public string MoneyFlow => TransferType?.ToLower() switch
        {
            "in" => "MONEY_IN",
            "out" => "MONEY_OUT",
            _ => "UNKNOWN"
        };

        /// <summary>
        /// Returns a clean representation of the callback for logging
        /// </summary>
        public override string ToString()
        {
            return $"SePay Callback: {CallbackType} | {MoneyFlow} | {Amount:C} | TxId: {TransactionId} | OrderCode: {OrderCode}";
        }
        #endregion
    }
}