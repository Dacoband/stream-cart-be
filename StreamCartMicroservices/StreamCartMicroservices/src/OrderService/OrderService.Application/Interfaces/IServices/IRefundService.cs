using OrderService.Application.DTOs.RefundDTOs;
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
    }
}