using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Data;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class RefundRequestRepository : EfCoreGenericRepository<RefundRequest>, IRefundRequestRepository
    {
        private readonly OrderContext _context;

        public RefundRequestRepository(OrderContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RefundRequest>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.RefundRequests
                .Include(r => r.RefundDetails)
                .Where(r => r.OrderId == orderId && !r.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// ✅ THÊM METHOD THIẾU - Gets refund requests by user ID
        /// </summary>
        public async Task<IEnumerable<RefundRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.RefundRequests
                .Include(r => r.RefundDetails)
                .Where(r => r.RequestedByUserId == userId && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefundRequest>> GetByStatusAsync(RefundStatus status)
        {
            return await _context.RefundRequests
                .Include(r => r.RefundDetails)
                .Where(r => r.Status == status && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<PagedResult<RefundRequest>> GetPagedRefundRequestsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            RefundStatus? status = null,
            Guid? orderId = null,
            Guid? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.RefundRequests
                .Include(r => r.RefundDetails)
                .Where(r => !r.IsDeleted);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (orderId.HasValue)
                query = query.Where(r => r.OrderId == orderId.Value);

            if (userId.HasValue)
                query = query.Where(r => r.RequestedByUserId == userId.Value);

            if (fromDate.HasValue)
                query = query.Where(r => r.RequestedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.RequestedAt <= toDate.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<RefundRequest>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<RefundRequest?> GetWithDetailsAsync(Guid refundRequestId)
        {
            return await _context.RefundRequests
                .Include(r => r.RefundDetails)
                .FirstOrDefaultAsync(r => r.Id == refundRequestId && !r.IsDeleted);
        }

        /// <summary>
        /// ✅ Method cho background job - lấy refund requests có tracking code
        /// </summary>
        public async Task<IEnumerable<RefundRequest>> GetRefundRequestsWithTrackingCodeAsync()
        {
            return await _context.RefundRequests
                .Include(r => r.RefundDetails)
                .Where(r => !r.IsDeleted &&
                           !string.IsNullOrEmpty(r.TrackingCode) &&
                           r.Status == RefundStatus.Packed) // ✅ Chỉ lấy những refund đang Packed
                .ToListAsync();
        }
    }
}