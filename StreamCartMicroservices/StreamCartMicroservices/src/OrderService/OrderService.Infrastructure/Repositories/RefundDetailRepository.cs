using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data;
using Shared.Common.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class RefundDetailRepository : EfCoreGenericRepository<RefundDetail>, IRefundDetailRepository
    {
        private readonly OrderContext _context;

        public RefundDetailRepository(OrderContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets refund details by refund request ID
        /// </summary>
        public async Task<IEnumerable<RefundDetail>> GetByRefundRequestIdAsync(Guid refundRequestId)
        {
            return await _context.RefundDetails
                .Where(rd => rd.RefundRequestId == refundRequestId && !rd.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Gets refund details by order item ID
        /// </summary>
        public async Task<IEnumerable<RefundDetail>> GetByOrderItemIdAsync(Guid orderItemId)
        {
            return await _context.RefundDetails
                .Where(rd => rd.OrderItemId == orderItemId && !rd.IsDeleted)
                .ToListAsync();
        }
    }
}