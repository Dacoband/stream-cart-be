using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.DTOs
{
    /// <summary>
    /// Request DTO cho việc phê duyệt rút tiền
    /// </summary>
    public class WithdrawalApprovalRequestDto
    {
        /// <summary>
        /// ID của wallet transaction cần phê duyệt
        /// </summary>
        [Required(ErrorMessage = "Wallet Transaction ID là bắt buộc")]
        public Guid WalletTransactionId { get; set; }

        /// <summary>
        /// Ghi chú phê duyệt (tùy chọn)
        /// </summary>
        public string? ApprovalNote { get; set; }
    }
}