using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.DTOs
{
    /// <summary>
    /// DTO cho yêu cầu nạp tiền vào ví
    /// </summary>
    public class DepositRequestDto
    {
        /// <summary>
        /// Số tiền nạp vào ví
        /// </summary>
        [Required]
        [Range(10000, 50000000, ErrorMessage = "Số tiền nạp phải từ 10.000đ đến 50.000.000đ")]
        public decimal Amount { get; set; }

        /// <summary>
        /// ID shop cần nạp tiền (optional, nếu không có sẽ lấy từ JWT)
        /// </summary>
        public Guid? ShopId { get; set; }

        /// <summary>
        /// Mô tả giao dịch (optional)
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Response cho deposit
    /// </summary>
    public class DepositResponseDto
    {
        /// <summary>
        /// Mã QR code để thanh toán
        /// </summary>
        public string QrCode { get; set; } = string.Empty;

        /// <summary>
        /// ID payment được tạo
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Số tiền nạp
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ID shop
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// Mô tả giao dịch
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}