namespace PaymentService.Application.DTOs
{
    /// <summary>
    /// Response DTO cho việc phê duyệt rút tiền
    /// </summary>
    public class WithdrawalApprovalResponseDto
    {
        /// <summary>
        /// ID payment được tạo
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// ID wallet transaction
        /// </summary>
        public Guid WalletTransactionId { get; set; }

        /// <summary>
        /// QR code để chuyển tiền
        /// </summary>
        public string QrCode { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền cần chuyển
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string? BankAccount { get; set; }

        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        public string? BankNumber { get; set; }

        /// <summary>
        /// Mô tả giao dịch
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}