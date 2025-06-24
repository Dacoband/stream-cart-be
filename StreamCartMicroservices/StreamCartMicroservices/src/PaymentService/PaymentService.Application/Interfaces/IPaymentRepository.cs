using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        /// <summary>
        /// Lấy payment theo ID của đơn hàng
        /// </summary>
        Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Lấy danh sách payment theo ID người dùng
        /// </summary>
        Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy payment theo mã giao dịch từ payment provider
        /// </summary>
        Task<Payment?> GetByQrCodeAsync(string qrCode);


        /// <summary>
        /// Lấy danh sách payment theo phương thức thanh toán
        /// </summary>
        Task<IEnumerable<Payment>> GetByPaymentMethodAsync(PaymentMethod paymentMethod);

        /// <summary>
        /// Lấy danh sách payment theo trạng thái
        /// </summary>
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);

        /// <summary>
        /// Lấy danh sách payment trong khoảng thời gian
        /// </summary>
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy danh sách payment phân trang
        /// </summary>
        Task<PagedResult<Payment>> GetPagedPaymentsAsync(
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
        /// Lấy tổng số tiền thanh toán trong khoảng thời gian
        /// </summary>
        Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy thống kê payment theo trạng thái
        /// </summary>
        Task<Dictionary<PaymentStatus, int>> GetPaymentCountByStatusAsync();

        /// <summary>
        /// Lấy thống kê payment theo phương thức thanh toán
        /// </summary>
        Task<Dictionary<PaymentMethod, int>> GetPaymentCountByMethodAsync();
    }
}