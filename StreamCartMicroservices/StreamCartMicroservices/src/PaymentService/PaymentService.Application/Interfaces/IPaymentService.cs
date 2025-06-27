using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Tạo payment mới
        /// </summary>
        /// <param name="createPaymentDto">Thông tin payment mới</param>
        /// <returns>Payment đã tạo</returns>
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);

        /// <summary>
        /// Lấy thông tin payment theo ID
        /// </summary>
        /// <param name="paymentId">ID của payment</param>
        /// <returns>Payment nếu tìm thấy, null nếu không tìm thấy</returns>
        Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId);

        /// <summary>
        /// Lấy danh sách payment phân trang
        /// </summary>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Số lượng mỗi trang</param>
        /// <param name="status">Trạng thái payment (tùy chọn)</param>
        /// <param name="method">Phương thức thanh toán (tùy chọn)</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <param name="orderId">ID đơn hàng (tùy chọn)</param>
        /// <param name="fromDate">Từ ngày (tùy chọn)</param>
        /// <param name="toDate">Đến ngày (tùy chọn)</param>
        /// <param name="sortBy">Sắp xếp theo (tùy chọn)</param>
        /// <param name="ascending">Sắp xếp tăng dần hay không (tùy chọn)</param>
        /// <returns>Kết quả phân trang</returns>
        Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(
            int pageNumber,
            int pageSize,
            PaymentStatus? status = null,
            PaymentMethod? method = null,
            Guid? userId = null,
            Guid? orderId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? sortBy = null,
            bool ascending = true);

        /// <summary>
        /// Cập nhật trạng thái payment
        /// </summary>
        /// <param name="paymentId">ID của payment</param>
        /// <param name="updateStatusDto">Thông tin cập nhật</param>
        /// <returns>Payment đã cập nhật</returns>
        Task<PaymentDto?> UpdatePaymentStatusAsync(Guid paymentId, UpdatePaymentStatusDto updateStatusDto);

        /// <summary>
        /// Xử lý callback từ payment provider
        /// </summary>
        /// <param name="paymentId">ID của payment</param>
        /// <param name="callbackDto">Thông tin callback</param>
        /// <returns>Payment đã cập nhật</returns>
        Task<PaymentDto?> ProcessPaymentCallbackAsync(Guid paymentId, PaymentCallbackDto callbackDto);

        /// <summary>
        /// Yêu cầu hoàn tiền
        /// </summary>
        /// <param name="paymentId">ID của payment</param>
        /// <param name="refundDto">Thông tin hoàn tiền</param>
        /// <returns>Payment đã cập nhật</returns>
        Task<PaymentDto?> RefundPaymentAsync(Guid paymentId, RefundPaymentDto refundDto);

        /// <summary>
        /// Lấy lịch sử thanh toán của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Danh sách payment</returns>
        Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy thanh toán của đơn hàng
        /// </summary>
        /// <param name="orderId">ID đơn hàng</param>
        /// <returns>Danh sách payment</returns>
        Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Xóa payment (soft delete)
        /// </summary>
        /// <param name="paymentId">ID của payment</param>
        /// <param name="deletedBy">Người xóa</param>
        /// <returns>True nếu xóa thành công, false nếu không tìm thấy payment</returns>
        Task<bool> DeletePaymentAsync(Guid paymentId, string deletedBy);

        /// <summary>
        /// Lấy thống kê payment
        /// </summary>
        /// <param name="fromDate">Từ ngày (tùy chọn)</param>
        /// <param name="toDate">Đến ngày (tùy chọn)</param>
        /// <returns>Thông tin thống kê</returns>
        Task<PaymentSummaryDto> GetPaymentSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}