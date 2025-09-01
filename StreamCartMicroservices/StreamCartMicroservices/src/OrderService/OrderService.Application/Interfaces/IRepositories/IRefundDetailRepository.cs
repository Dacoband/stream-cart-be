using OrderService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces.IRepositories
{
    public interface IRefundDetailRepository : IGenericRepository<RefundDetail>
    {
        /// <summary>
        /// Gets refund details by refund request ID
        /// </summary>
        Task<IEnumerable<RefundDetail>> GetByRefundRequestIdAsync(Guid refundRequestId);

        /// <summary>
        /// Gets refund details by order item ID
        /// </summary>
        Task<IEnumerable<RefundDetail>> GetByOrderItemIdAsync(Guid orderItemId);
    }
}