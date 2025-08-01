using System.ComponentModel.DataAnnotations;

namespace AccountService.Application.DTOs
{
    public class DeactivateModeratorDto
    {
        [Required(ErrorMessage = "Lý do vô hiệu hóa là bắt buộc")]
        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Có gửi thông báo cho moderator không
        /// </summary>
        public bool SendNotification { get; set; } = true;

        /// <summary>
        /// Thời gian vô hiệu hóa (tùy chọn)
        /// </summary>
        public DateTime? DeactivateUntil { get; set; }
    }
}