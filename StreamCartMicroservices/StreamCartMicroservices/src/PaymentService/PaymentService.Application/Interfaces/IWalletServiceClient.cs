using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces
{
    /// <summary>
    /// Client để gọi WalletService từ PaymentService
    /// </summary>
    public interface IWalletServiceClient
    {
        /// <summary>
        /// Tạo giao dịch ví (wallet transaction)
        /// </summary>
        /// <param name="request">Thông tin giao dịch</param>
        /// <returns>True nếu thành công</returns>
        Task<bool> CreateWalletTransactionAsync(CreateWalletTransactionRequest request);

        /// <summary>
        /// Kiểm tra shop có tồn tại không
        /// </summary>
        /// <param name="shopId">ID shop</param>
        /// <returns>True nếu shop tồn tại</returns>
        Task<bool> DoesShopExistAsync(Guid shopId);
        Task<WalletTransactionDto?> GetWalletTransactionByIdAsync(Guid transactionId);

        /// <summary>
        /// Cập nhật trạng thái wallet transaction
        /// </summary>
        Task<bool> UpdateWalletTransactionStatusAsync(Guid transactionId, int status, string? paymentTransactionId = null, string? modifiedBy = null);
    }
}

    /// <summary>
    /// Request để tạo wallet transaction
    /// </summary>
    public class CreateWalletTransactionRequest
    {
        /// <summary>
        /// Loại giao dịch (1 = Deposit)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Số tiền giao dịch
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ID shop
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// Trạng thái giao dịch (0 = Success, 1 = Failed, 2 = Pending, 3 = Canceled)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// ID giao dịch thanh toán
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Mô tả giao dịch
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Người tạo giao dịch
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;
    
}