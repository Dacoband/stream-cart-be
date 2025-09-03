using ProductService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Interfaces
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Tạo mã QR code cho thanh toán từ thông tin đơn hàng
        /// </summary>
        /// <param name="orderId">ID đơn hàng</param>
        /// <param name="amount">Số tiền thanh toán</param>
        /// <param name="userId">ID người dùng</param>
        /// <param name="paymentMethod">Phương thức thanh toán</param>
        /// <returns>Chuỗi mã QR code</returns>
        Task<string> GenerateQrCodeAsync(Guid orderId, decimal amount, Guid userId, PaymentMethod paymentMethod);


        /// <summary>
        /// Tạo mã QR code cho thanh toán nhiều đơn hàng
        /// </summary>
        /// <param name="orderIds">Danh sách ID đơn hàng</param>
        /// <param name="amount">Tổng số tiền thanh toán</param>
        /// <param name="userId">ID người dùng</param>
        /// <param name="paymentMethod">Phương thức thanh toán</param>
        /// <returns>Chuỗi mã QR code</returns>
        Task<string> GenerateBulkQrCodeAsync(List<Guid> orderIds, decimal amount, Guid userId, PaymentMethod paymentMethod);

        /// <summary>
        /// Xác thực mã QR code đã tạo
        /// </summary>
        /// <param name="qrCode">Mã QR code cần xác thực</param>
        /// <returns>true nếu hợp lệ, false nếu không hợp lệ</returns>
        Task<bool> ValidateQrCodeAsync(string qrCode);
        Task<string> GenerateDepositQrCodeAsync(Guid shopId, decimal amount, Guid userId, PaymentMethod paymentMethod);
        Task<string> GenerateWithdrawalQrCodeAsync(Guid walletTransactionId, decimal amount, Guid userId, PaymentMethod paymentMethod, string? bankAccount, string? bankNumber);

        Task<string> GenerateRefundQrCodeAsync(Guid refundRequestId, decimal amount, Guid userId, PaymentMethod paymentMethod, string bankName, string bankNumber);

    }
}
