using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBoxService.Application.DTOs
{
    public class ChatbotRequestDTO
    {
        [Required(ErrorMessage = "Tin nhắn của khách hàng là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Tin nhắn không được vượt quá 1000 ký tự")]
        public string CustomerMessage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shop ID là bắt buộc")]
        public Guid ShopId { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? ChatRoomId { get; set; }

        /// <summary>
        /// Context from previous conversation for better responses
        /// </summary>
        public string? ConversationContext { get; set; }

        /// <summary>
        /// Customer's preferred language (vi, en)
        /// </summary>
        public string Language { get; set; } = "vi";
    }
}
