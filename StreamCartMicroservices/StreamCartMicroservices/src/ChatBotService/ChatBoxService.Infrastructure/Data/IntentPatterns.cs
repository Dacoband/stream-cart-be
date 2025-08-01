using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChatBoxService.Infrastructure.Data
{
    public static class IntentPatterns
    {
        public static readonly Dictionary<string, List<string>> PatternsByIntent = new()
        {
            ["greeting"] = new List<string>
            {
                @"^(xin\s*chào|chào|hello|hi|hey|hola).*",
                @"^(chào\s*buổi\s*(sáng|chiều|tối)).*",
                @"^có\s*ai\s*(ở\s*đó|đây|giúp|tư\s*vấn).*",
                @"^(anh|chị|bạn|em|ad|admin|shop|ơi|à|ạ)$"
            },

            ["thanks"] = new List<string>
            {
                @"(cảm\s*ơn|cám\s*ơn|thank|thanks|thank\s*you)",
                @"(cảm\s*tạ|biết\s*ơn|nhờ|may\s*quá)"
            },

             ["shop_search"] = new List<string>
{
    @"(sản\s*phẩm|hàng\s*hóa|mặt\s*hàng)\s*(của|ở|tại|thuộc)\s*(shop|cửa\s*hàng)",
    @"(shop|cửa\s*hàng)\s*(.+?)\s*(có|bán|cung\s*cấp)\s*(gì|những\s*gì|sản\s*phẩm)",
    // Patterns hiện tại
    @"(tìm|tìm\s*kiếm|kiếm|có)\s*(shop|cửa\s*hàng|store|chỗ|nơi)\s*(bán|có|mua|shop)",
    @"(shop|cửa\s*hàng|store)\s*(nào|có|bán)",
    @"(mua|kiếm|tìm)\s*ở\s*(đâu|shop|cửa\s*hàng)",
    @"(shop|cửa\s*hàng)\s*(uy\s*tín|nổi\s*tiếng|tốt|nhiều\s*sản\s*phẩm)",
@"(shop|cửa\s*hàng)\s*(nào|ở đâu)\s*(bán|có)\s*(.+)",
@"(tìm|kiếm)\s*(shop|cửa\s*hàng)\s*(bán|có)\s*(.+)"
},

            ["product_search"] = new List<string>
            {
                @"(tìm|tìm\s*kiếm|kiếm|có)\s*(sản\s*phẩm|mặt\s*hàng|hàng|món)",
                @"(mua|bán|cần|muốn)\s*(sản\s*phẩm|cái|chiếc|món|mặt\s*hàng)",
                @"(có\s*bán|bán|tìm|tìm\s*mua)",
                @"(mua|tìm|kiếm)\s*(đồ|đồ\s*dùng|vật\s*dụng)"
            },

            ["price_inquiry"] = new List<string>
            {
                @"(giá|giá\s*cả|giá\s*tiền|bao\s*nhiêu\s*tiền|mấy\s*tiền)",
                @"(bao\s*nhiêu|bao\s*lăm|bao\s*nhiu)",
                @"(đắt|rẻ|mắc|giá\s*thế\s*nào)",
                @"(rẻ\s*nhất|tốt\s*nhất|hời\s*nhất)"
            },

            ["recommendation"] = new List<string>
            {
                @"(gợi\s*ý|tư\s*vấn|khuyên|đề\s*xuất|recommend)",
                @"(nên\s*mua|tốt\s*nhất|phù\s*hợp)",
                @"(chọn|lựa\s*chọn|lựa|chọn\s*lựa)",
                @"(thích\s*hợp|hợp|nên\s*chọn)"
            }
        };

        // Utility method to detect intent from message
        public static string DetectIntent(string message, out decimal confidence)
        {
            confidence = 0.0m;
            if (string.IsNullOrWhiteSpace(message))
                return "general_question";

            var normalizedMessage = message.ToLower().Trim();

            foreach (var intentPattern in PatternsByIntent)
            {
                var intent = intentPattern.Key;
                var patterns = intentPattern.Value;

                foreach (var pattern in patterns)
                {
                    if (Regex.IsMatch(normalizedMessage, pattern))
                    {
                        confidence = 0.9m;
                        return intent;
                    }
                }
            }

            // Product keywords detection
            var productKeywords = new[] { "điện thoại", "laptop", "máy tính", "quần áo", "giày dép", "thời trang" };
            if (productKeywords.Any(k => normalizedMessage.Contains(k)))
            {
                // If contains shop or store, likely shop search
                if (normalizedMessage.Contains("shop") || normalizedMessage.Contains("cửa hàng") || normalizedMessage.Contains("store"))
                {
                    confidence = 0.85m;
                    return "shop_search";
                }

                confidence = 0.75m;
                return "product_search";
            }

            confidence = 0.6m;
            return "general_question";
        }
    }
}