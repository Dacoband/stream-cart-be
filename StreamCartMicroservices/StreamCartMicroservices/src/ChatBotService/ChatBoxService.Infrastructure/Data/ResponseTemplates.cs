using System;
using System.Collections.Generic;

namespace ChatBoxService.Infrastructure.Data
{
    public static class ResponseTemplates
    {
        public static readonly Dictionary<string, List<string>> GreetingResponses = new Dictionary<string, List<string>>
        {
            ["default"] = new List<string>
            {
                "Xin chào anh/chị! Chào mừng đến với StreamCart. Tôi có thể giúp gì cho anh/chị hôm nay? 😊",
                "Chào anh/chị! Rất vui được hỗ trợ anh/chị trên StreamCart. Anh/chị cần tìm sản phẩm nào?",
                "Xin chào! Tôi là StreamCart AI, sẵn sàng hỗ trợ anh/chị tìm kiếm sản phẩm ưng ý!"
            },
            ["morning"] = new List<string>
            {
                "Chào buổi sáng anh/chị! Tôi có thể giúp gì cho anh/chị trên StreamCart hôm nay?",
                "Xin chào! Chúc anh/chị một buổi sáng tốt lành. Tôi có thể giúp gì cho anh/chị?"
            },
            ["evening"] = new List<string>
            {
                "Chào buổi tối anh/chị! Tôi có thể hỗ trợ anh/chị tìm kiếm sản phẩm không?",
                "Xin chào! Đêm nay anh/chị cần mua sắm gì trên StreamCart? Tôi sẵn sàng giúp đỡ!"
            }
        };

        public static readonly Dictionary<string, List<string>> ProductSearchResponses = new Dictionary<string, List<string>>
        {
            ["success"] = new List<string>
            {
                "Tôi đã tìm thấy những sản phẩm sau đây cho anh/chị trên StreamCart:\n\n{PRODUCTS}\n\nAnh/chị có muốn xem thêm thông tin về sản phẩm nào không?",
                "Đây là các sản phẩm phù hợp với yêu cầu của anh/chị:\n\n{PRODUCTS}\n\nAnh/chị cần tư vấn thêm về sản phẩm nào?"
            },
            ["empty"] = new List<string>
            {
                "Xin lỗi, tôi không tìm thấy sản phẩm phù hợp với yêu cầu của anh/chị. Anh/chị có thể thử với từ khóa khác không?",
                "Rất tiếc, hiện tại không có sản phẩm nào khớp với yêu cầu tìm kiếm. Anh/chị có thể cho biết thêm chi tiết hoặc thử từ khóa khác?"
            }
        };

        public static readonly Dictionary<string, List<string>> ShopSearchResponses = new Dictionary<string, List<string>>
        {
            ["success"] = new List<string>
            {
                "Tôi đã tìm thấy những cửa hàng sau đây có bán sản phẩm anh/chị quan tâm:\n\n{SHOPS}\n\nAnh/chị muốn xem thêm thông tin về cửa hàng nào?",
                "Đây là các cửa hàng uy tín trên StreamCart có bán sản phẩm anh/chị đang tìm:\n\n{SHOPS}\n\nMỗi cửa hàng đều có đánh giá tốt từ người mua!"
            },
            ["empty"] = new List<string>
            {
                "Xin lỗi, hiện tại tôi không tìm thấy cửa hàng nào phù hợp với yêu cầu của anh/chị. Anh/chị có thể thử tìm kiếm với từ khóa khác không?",
                "Rất tiếc, chưa có cửa hàng nào trên StreamCart phù hợp với tìm kiếm của anh/chị. Anh/chị có thể cho biết loại sản phẩm cụ thể hơn không?"
            }
        };

        public static readonly Dictionary<string, List<string>> ThankYouResponses = new Dictionary<string, List<string>>
        {
            ["default"] = new List<string>
            {
                "Dạ không có gì ạ! Rất vui được hỗ trợ anh/chị. Anh/chị cần giúp đỡ gì nữa không?",
                "Rất hân hạnh được phục vụ anh/chị! Nếu có bất kỳ câu hỏi nào khác, đừng ngần ngại hỏi tôi nhé!",
                "Không có chi ạ! StreamCart luôn sẵn sàng hỗ trợ anh/chị mọi lúc!"
            }
        };

        public static readonly Dictionary<string, List<string>> FallbackResponses = new Dictionary<string, List<string>>
        {
            ["default"] = new List<string>
            {
                "Xin lỗi, tôi chưa hiểu rõ yêu cầu của anh/chị. Anh/chị có thể diễn đạt lại được không?",
                "Tôi xin lỗi nhưng tôi không chắc mình hiểu đúng ý anh/chị. Anh/chị có thể cho biết cụ thể hơn về điều anh/chị đang tìm kiếm không?",
                "Xin lỗi vì sự bất tiện này. Để tôi hỗ trợ tốt hơn, anh/chị có thể cho tôi biết anh/chị muốn tìm sản phẩm gì hoặc cần thông tin về shop nào không?"
            }
        };

        // Utility function to get random response from templates
        public static string GetRandomResponse(string intentType, string variant = "default")
        {
            var responses = intentType switch
            {
                "greeting" => GreetingResponses,
                "product_search" => ProductSearchResponses,
                "shop_search" => ShopSearchResponses,
                "thanks" => ThankYouResponses,
                _ => FallbackResponses
            };

            if (!responses.ContainsKey(variant))
                variant = "default";

            if (!responses.ContainsKey(variant))
                return "Xin chào! Tôi có thể giúp gì cho anh/chị?";

            var options = responses[variant];
            var random = new Random();
            return options[random.Next(options.Count)];
        }
    }
}