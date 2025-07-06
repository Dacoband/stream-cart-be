using System;

namespace OrderService.Application.DTOs.WalletDTOs
{
    /// <summary>
    /// Yêu cầu thanh toán cho shop sau khi đơn hàng hoàn thành
    /// </summary>
    public class ShopPaymentRequest
    {
        /// <summary>
        /// ID đơn hàng
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// ID của shop
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// Số tiền thanh toán cho shop (đã trừ phí)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Phí nền tảng (10% tổng giá trị đơn hàng)
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Loại giao dịch
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch
        /// </summary>
        public string TransactionReference { get; set; }

        /// <summary>
        /// Mô tả giao dịch
        /// </summary>
        public string Description { get; set; }
    }
}