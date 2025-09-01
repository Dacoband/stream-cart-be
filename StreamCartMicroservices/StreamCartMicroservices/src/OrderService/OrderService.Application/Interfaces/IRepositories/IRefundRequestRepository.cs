using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Data.Interfaces;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces.IRepositories
{
    public interface IRefundRequestRepository : IGenericRepository<RefundRequest>
    {
        /// <summary>
        /// Gets refund requests by order ID
        /// </summary>
        Task<IEnumerable<RefundRequest>> GetByOrderIdAsync(Guid orderId);

        /// <summary>
        /// Gets refund requests by user ID
        /// </summary>
        Task<IEnumerable<RefundRequest>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Gets refund requests by status
        /// </summary>
        Task<IEnumerable<RefundRequest>> GetByStatusAsync(RefundStatus status);

        /// <summary>
        /// Gets paged refund requests
        /// </summary>
        Task<PagedResult<RefundRequest>> GetPagedRefundRequestsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            RefundStatus? status = null,
            Guid? orderId = null,
            Guid? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Gets refund request with details
        /// </summary>
        Task<RefundRequest?> GetWithDetailsAsync(Guid refundRequestId);
        Task<IEnumerable<RefundRequest>> GetRefundRequestsWithTrackingCodeAsync();
    }
}