using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;
using ProductService.Domain.Enums;
using Shared.Common.Data.Repositories;
using Shared.Common.Domain.Bases;

namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentRepository : EfCoreGenericRepository<Payment>, IPaymentRepository
    {
        private readonly PaymentContext _context;

        public PaymentRepository(PaymentContext paymentContext) : base(paymentContext)
        {
            _context = paymentContext;
        }

        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .ToListAsync();
        }

        // Sửa method GetByTransactionIdAsync thành GetByQrCodeAsync
        public async Task<Payment?> GetByQrCodeAsync(string qrCode)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.QrCode == qrCode && !p.IsDeleted);
        }

        public async Task<IEnumerable<Payment>> GetByPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            return await _context.Payments
                .Where(p => p.PaymentMethod == paymentMethod && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .Where(p => p.Status == status && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<PagedResult<Payment>> GetPagedPaymentsAsync(
            int pageNumber,
            int pageSize,
            PaymentStatus? status = null,
            PaymentMethod? method = null,
            Guid? userId = null,
            Guid? orderId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? sortBy = null,
            bool ascending = true)
        {
            // Start with base query
            var query = _context.Payments.Where(p => !p.IsDeleted);

            // Apply filters
            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (method.HasValue)
                query = query.Where(p => p.PaymentMethod == method.Value);

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            if (orderId.HasValue)
                query = query.Where(p => p.OrderId == orderId.Value);

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt <= toDate.Value);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                // Because this is dynamic sorting based on a string field name,
                // we need conditional logic to apply the correct ordering expression
                switch (sortBy.ToLower())
                {
                    case "amount":
                        query = ascending
                            ? query.OrderBy(p => p.Amount)
                            : query.OrderByDescending(p => p.Amount);
                        break;
                    case "createdat":
                        query = ascending
                            ? query.OrderBy(p => p.CreatedAt)
                            : query.OrderByDescending(p => p.CreatedAt);
                        break;
                    case "status":
                        query = ascending
                            ? query.OrderBy(p => p.Status)
                            : query.OrderByDescending(p => p.Status);
                        break;
                    case "paymentmethod":
                        query = ascending
                            ? query.OrderBy(p => p.PaymentMethod)
                            : query.OrderByDescending(p => p.PaymentMethod);
                        break;
                    case "processedat":
                        query = ascending
                            ? query.OrderBy(p => p.ProcessedAt)
                            : query.OrderByDescending(p => p.ProcessedAt);
                        break;
                    default:
                        query = ascending
                            ? query.OrderBy(p => p.CreatedAt)
                            : query.OrderByDescending(p => p.CreatedAt);
                        break;
                }
            }
            else
            {
                // Default sort by creation date, newest first
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            // Apply pagination
            var payments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Payment>
            {
                Items = payments,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.CreatedAt >= startDate &&
                       p.CreatedAt <= endDate &&
                       p.Status == PaymentStatus.Paid &&
                       !p.IsDeleted)
                .SumAsync(p => p.Amount);
        }

        public async Task<Dictionary<PaymentStatus, int>> GetPaymentCountByStatusAsync()
        {
            var statusCounts = await _context.Payments
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .ToDictionary(
                    status => status,
                    status => statusCounts.FirstOrDefault(p => p.Status == status)?.Count ?? 0
                );

            return result;
        }

        public async Task<Dictionary<PaymentMethod, int>> GetPaymentCountByMethodAsync()
        {
            var methodCounts = await _context.Payments
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = Enum.GetValues(typeof(PaymentMethod))
                .Cast<PaymentMethod>()
                .ToDictionary(
                    method => method,
                    method => methodCounts.FirstOrDefault(p => p.Method == method)?.Count ?? 0
                );

            return result;
        }
    }
}