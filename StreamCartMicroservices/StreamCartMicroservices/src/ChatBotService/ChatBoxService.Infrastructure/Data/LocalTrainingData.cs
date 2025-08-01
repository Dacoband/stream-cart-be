using System.Collections.Generic;

namespace ChatBoxService.Infrastructure.Data
{
    public static class LocalTrainingData
    {
        // Frequently asked questions and answers
        public static readonly Dictionary<string, string> FAQs = new()
        {
            ["StreamCart là gì?"] = "StreamCart là nền tảng thương mại điện tử kết hợp với livestream, giúp người bán có thể tiếp cận khách hàng trực tiếp thông qua video trực tuyến. Trên StreamCart, bạn có thể mua sắm từ hàng ngàn cửa hàng uy tín với nhiều loại sản phẩm đa dạng. 🛍️",

            ["Làm sao để đặt hàng trên StreamCart?"] = "Để đặt hàng trên StreamCart, anh/chị chỉ cần làm theo các bước đơn giản: (1) Tìm sản phẩm yêu thích, (2) Nhấn 'Thêm vào giỏ hàng' hoặc 'Mua ngay', (3) Điền thông tin giao hàng, (4) Chọn phương thức thanh toán, (5) Xác nhận đơn hàng. Sau khi đặt hàng thành công, anh/chị có thể theo dõi trạng thái đơn hàng trong mục 'Đơn hàng của tôi'. 📦",

            ["Chính sách đổi trả của StreamCart?"] = "StreamCart có chính sách đổi trả linh hoạt trong vòng 7 ngày kể từ khi nhận hàng nếu sản phẩm bị lỗi do nhà sản xuất hoặc không đúng mô tả. Để đổi trả, anh/chị vui lòng liên hệ với cửa hàng hoặc bộ phận hỗ trợ khách hàng của StreamCart qua mục 'Trợ giúp'. 🔄",

            ["Làm sao để trở thành người bán trên StreamCart?"] = "Để trở thành người bán trên StreamCart, anh/chị cần đăng ký tài khoản StreamCart, sau đó chọn 'Đăng ký bán hàng' và cung cấp thông tin cần thiết như thông tin cá nhân, thông tin kinh doanh và tài khoản ngân hàng. Sau khi được phê duyệt, anh/chị có thể bắt đầu đăng sản phẩm và livestream bán hàng! 🏪",

            ["Phí vận chuyển trên StreamCart?"] = "Phí vận chuyển trên StreamCart được tính dựa trên khoảng cách, trọng lượng sản phẩm và đơn vị vận chuyển. Nhiều shop trên StreamCart còn có chương trình miễn phí vận chuyển cho đơn hàng từ một giá trị nhất định. Anh/chị có thể xem phí vận chuyển chính xác khi thêm sản phẩm vào giỏ hàng và nhập địa chỉ giao hàng. 🚚",

            ["Phương thức thanh toán nào được chấp nhận?"] = "StreamCart chấp nhận nhiều phương thức thanh toán như: thẻ tín dụng/ghi nợ, ví điện tử (MoMo, ZaloPay, VNPay), chuyển khoản ngân hàng và thanh toán khi nhận hàng (COD). Anh/chị có thể chọn phương thức phù hợp nhất khi thanh toán đơn hàng. 💳",

            ["Làm sao để liên hệ hỗ trợ khách hàng?"] = "Anh/chị có thể liên hệ hỗ trợ khách hàng của StreamCart qua nhiều kênh: chat trực tuyến trên website/app, email support@streamcart.vn, hotline 1900xxxx (7:00-22:00 mỗi ngày), hoặc fanpage Facebook của StreamCart. Đội ngũ hỗ trợ của chúng tôi luôn sẵn sàng giúp đỡ anh/chị! 📞",

            ["StreamCart có an toàn không?"] = "Có, StreamCart là nền tảng mua sắm an toàn với hệ thống bảo mật thanh toán tiên tiến, xác thực người bán, và đánh giá người dùng. Chúng tôi còn có chính sách bảo vệ người mua, đảm bảo anh/chị nhận được đúng sản phẩm như mô tả hoặc được hoàn tiền. 🔒"
        };

        // Product recommendations by category
        public static readonly Dictionary<string, List<string>> CategoryRecommendations = new()
        {
            ["điện thoại"] = new List<string> {
                "iPhone 15 Pro Max",
                "Samsung Galaxy S24 Ultra",
                "Xiaomi Redmi Note 13 Pro",
                "OPPO Reno 12",
                "Vivo V30"
            },

            ["laptop"] = new List<string> {
                "MacBook Air M3",
                "Dell XPS 13",
                "HP Spectre x360",
                "Lenovo ThinkPad X1 Carbon",
                "ASUS ROG Zephyrus G14"
            },

            ["thời trang"] = new List<string> {
                "Áo thun unisex",
                "Áo sơ mi nam",
                "Đầm suông nữ",
                "Quần jean slim fit",
                "Váy liền thân công sở"
            },

            ["mỹ phẩm"] = new List<string> {
                "Son môi 3CE",
                "Phấn nước Laneige",
                "Kem chống nắng Anessa",
                "Nước tẩy trang Bioderma",
                "Serum The Ordinary"
            }
        };

        // Product comparison templates
        public static readonly Dictionary<string, string> ComparisonTemplates = new()
        {
            ["điện thoại"] = "Khi so sánh điện thoại, anh/chị nên xem xét các yếu tố: (1) Hiệu năng chip, (2) Camera, (3) Dung lượng pin, (4) Màn hình, (5) Hệ điều hành, và (6) Giá cả. iPhone thường mạnh về hệ sinh thái và camera, Samsung nổi bật với màn hình và tính năng đa nhiệm, trong khi các thương hiệu Trung Quốc như Xiaomi, OPPO thường có giá cả cạnh tranh hơn.",

            ["laptop"] = "Khi chọn laptop, anh/chị nên cân nhắc: (1) CPU (Intel Core i5/i7 hoặc AMD Ryzen 5/7 trở lên cho hiệu suất tốt), (2) RAM (tối thiểu 8GB, lý tưởng là 16GB), (3) Ổ cứng SSD, (4) Card đồ họa (nếu làm đồ họa/chơi game), (5) Trọng lượng và thời lượng pin (nếu di chuyển nhiều).",

            ["máy ảnh"] = "Khi chọn máy ảnh, nên xem xét: (1) Loại máy (DSLR hay Mirrorless), (2) Cảm biến (Full-frame hay Crop), (3) Độ phân giải, (4) Dải ISO, (5) Tốc độ chụp liên tiếp, (6) Khả năng quay video, (7) Hệ thống lấy nét. Sony và Fujifilm mạnh về mirrorless, Canon và Nikon có hệ sinh thái ống kính đa dạng."
        };

        // Shopping tips
        public static readonly List<string> ShoppingTips = new()
        {
            "💡 Mẹo mua sắm: Đăng ký nhận thông báo từ shop yêu thích để không bỏ lỡ các chương trình khuyến mãi đặc biệt!",
            "💡 Mẹo tiết kiệm: Sử dụng mã giảm giá StreamCart và kết hợp với voucher của shop để được giảm giá tối đa!",
            "💡 Mẹo an toàn: Luôn kiểm tra đánh giá và phản hồi của người mua trước khi quyết định mua sản phẩm!",
            "💡 Mẹo mua sắm: Thêm sản phẩm vào giỏ hàng và đợi các sự kiện giảm giá lớn như 9.9, 11.11, 12.12 để được giá tốt nhất!",
            "💡 Mẹo tiết kiệm: Sử dụng tính năng so sánh giá của StreamCart để tìm được shop bán sản phẩm với giá tốt nhất!"
        };

        // Function to find best matching FAQ
        public static string FindBestMatchingFAQ(string query, out bool isGoodMatch)
        {
            isGoodMatch = false;
            if (string.IsNullOrWhiteSpace(query))
                return null;

            query = query.ToLower();

            // Try exact match first
            foreach (var faq in FAQs)
            {
                if (query.Contains(faq.Key.ToLower()))
                {
                    isGoodMatch = true;
                    return faq.Value;
                }
            }

            // Try partial match
            string bestMatch = null;
            int bestScore = 0;

            foreach (var faq in FAQs.Keys)
            {
                var words = faq.ToLower().Split(' ', ',', '?', '!', '.', ';');
                int score = words.Count(w => query.Contains(w));

                if (score > bestScore && score >= 2) // At least 2 matching words
                {
                    bestScore = score;
                    bestMatch = faq;
                    isGoodMatch = true;
                }
            }

            return bestMatch != null ? FAQs[bestMatch] : null;
        }

        // Function to get product recommendations by keyword
        public static List<string> GetProductRecommendations(List<string> keywords)
        {
            var recommendations = new List<string>();

            foreach (var keyword in keywords)
            {
                foreach (var category in CategoryRecommendations.Keys)
                {
                    if (keyword.Contains(category))
                    {
                        recommendations.AddRange(CategoryRecommendations[category]);
                        break;
                    }
                }
            }

            return recommendations.Count > 0 ? recommendations : null;
        }

        // Function to get random shopping tip
        public static string GetRandomShoppingTip()
        {
            var random = new Random();
            return ShoppingTips[random.Next(ShoppingTips.Count)];
        }
    }
}