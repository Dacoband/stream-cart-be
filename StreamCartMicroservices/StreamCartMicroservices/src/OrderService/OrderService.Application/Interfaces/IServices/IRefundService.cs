using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces.IServices
{
    public interface IRefundService
    {
        /// <summary>
        /// Creates a new refund request
        /// </summary>
        Task<RefundRequestDto> CreateRefundRequestAsync(CreateRefundRequestDto createRefundDto);

        /// <summary>
        /// Updates refund request status
        /// </summary>
        Task<RefundRequestDto> UpdateRefundStatusAsync(UpdateRefundStatusDto updateStatusDto);

        /// <summary>
        /// Gets refund request by ID
        /// </summary>
        Task<RefundRequestDto?> GetRefundRequestByIdAsync(Guid refundRequestId);

        /// <summary>
        /// Lấy danh sách refund của shop với filter
        /// </summary>
        Task<PagedResult<RefundRequestDto>> GetRefundRequestsByShopIdAsync(
            Guid shopId,
            int pageNumber = 1,
            int pageSize = 10,
            RefundStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        /// <summary>
        /// Seller xác nhận hoặc từ chối yêu cầu refund
        /// </summary>
        Task<RefundRequestDto> ConfirmRefundRequestAsync(
            Guid refundRequestId,
            bool isApproved,
            string? reason,
            string modifiedBy);
        Task<RefundRequestDto> UpdateRefundTransactionIdAsync(UpdateRefundTransactionDto updateTransactionDto);

    }
}