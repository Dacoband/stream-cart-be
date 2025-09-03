using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.RefundCommands;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Services.User;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Services
{
    public class RefundManagementService : IRefundService
    {
        private readonly IMediator _mediator;
        private readonly IRefundRequestRepository _refundRequestRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RefundManagementService> _logger;

        public RefundManagementService(
            IMediator mediator,
            IRefundRequestRepository refundRequestRepository,
            ICurrentUserService currentUserService,
            ILogger<RefundManagementService> logger)
        {
            _mediator = mediator;
            _refundRequestRepository = refundRequestRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<RefundRequestDto> CreateRefundRequestAsync(CreateRefundRequestDto createRefundDto)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                _logger.LogInformation("Creating refund request for order {OrderId} by user {UserId}",
                    createRefundDto.OrderId, userId);

                var command = new CreateRefundRequestCommand
                {
                    OrderId = createRefundDto.OrderId,
                    RefundItems = createRefundDto.RefundItems,
                    BankName = createRefundDto.BankName,        
                    BankNumber = createRefundDto.BankNumber,
                    RequestedBy = userId.ToString()
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request for order {OrderId}", createRefundDto.OrderId);
                throw;
            }
        }

        public async Task<RefundRequestDto> UpdateRefundStatusAsync(UpdateRefundStatusDto updateStatusDto)
        {
            try
            {
                _logger.LogInformation("Updating refund status for refund {RefundRequestId} to {NewStatus}",
                    updateStatusDto.RefundRequestId, updateStatusDto.NewStatus);

                var command = new UpdateRefundStatusCommand
                {
                    RefundRequestId = updateStatusDto.RefundRequestId,
                    NewStatus = updateStatusDto.NewStatus,
                   // ModifiedBy = updateStatusDto.ModifiedBy,
                    //TrackingCode = updateStatusDto.TrackingCode
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund status for refund {RefundRequestId}", updateStatusDto.RefundRequestId);
                throw;
            }
        }
        public async Task<RefundRequestDto> UpdateRefundTransactionIdAsync(UpdateRefundTransactionDto updateTransactionDto)
        {
            var command = new UpdateRefundTransactionCommand
            {
                RefundRequestId = updateTransactionDto.RefundRequestId,
                TransactionId = updateTransactionDto.TransactionId
            };

            return await _mediator.Send(command);
        }
        public async Task<RefundRequestDto?> GetRefundRequestByIdAsync(Guid refundRequestId)
        {
            try
            {
                _logger.LogInformation("Getting refund request {RefundRequestId}", refundRequestId);

                var refundRequest = await _refundRequestRepository.GetWithDetailsAsync(refundRequestId);
                if (refundRequest == null)
                    return null;

                return new RefundRequestDto
                {
                    Id = refundRequest.Id,
                    OrderId = refundRequest.OrderId,
                    TrackingCode = refundRequest.TrackingCode,
                    RequestedByUserId = refundRequest.RequestedByUserId,
                    RequestedAt = refundRequest.RequestedAt,
                    Status = refundRequest.Status,
                    ProcessedByUserId = refundRequest.ProcessedByUserId,
                    ProcessedAt = refundRequest.ProcessedAt,
                    RefundAmount = refundRequest.RefundAmount,
                    BankNumber= refundRequest.BankNumber,
                    BankName= refundRequest.BankName,
                    ShippingFee = refundRequest.ShippingFee,
                    TotalAmount = refundRequest.TotalAmount,
                    CreatedAt = refundRequest.CreatedAt,
                    CreatedBy = refundRequest.CreatedBy,
                    LastModifiedAt = refundRequest.LastModifiedAt,
                    LastModifiedBy = refundRequest.LastModifiedBy,
                    RefundDetails = refundRequest.RefundDetails.Select(rd => new RefundDetailDto
                    {
                        Id = rd.Id,
                        OrderItemId = rd.OrderItemId,
                        RefundRequestId = rd.RefundRequestId,
                        Reason = rd.Reason,
                        ImageUrl = rd.ImageUrl,
                        UnitPrice = rd.UnitPrice,
                        CreatedAt = rd.CreatedAt,
                        CreatedBy = rd.CreatedBy
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request {RefundRequestId}", refundRequestId);
                throw;
            }
        }
        public async Task<PagedResult<RefundRequestDto>> GetRefundRequestsByShopIdAsync(
             Guid shopId, int pageNumber = 1, int pageSize = 10,
             RefundStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                _logger.LogInformation("Getting refund requests for shop {ShopId}", shopId);

                var refunds = await _refundRequestRepository.GetPagedRefundRequestsAsync(
                    pageNumber, pageSize, status, null, null, fromDate, toDate);

                var refundDtos = refunds.Items.Select(ConvertToDto).ToList();

                return new PagedResult<RefundRequestDto>(
                    refundDtos,
                    refunds.TotalCount,
                    refunds.CurrentPage,
                    refunds.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund requests for shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<RefundRequestDto> ConfirmRefundRequestAsync(
            Guid refundRequestId, bool isApproved, string? reason, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Confirming refund request {RefundRequestId}, approved: {IsApproved}",
                    refundRequestId, isApproved);

                var refund = await _refundRequestRepository.GetByIdAsync(refundRequestId.ToString());
                if (refund == null)
                    throw new InvalidOperationException("Refund request not found");

                // ✅ Sử dụng enum values có sẵn
                var newStatus = isApproved ? RefundStatus.Confirmed : RefundStatus.Rejected;
                refund.UpdateStatus(newStatus, modifiedBy);

                await _refundRequestRepository.ReplaceAsync(refund.Id.ToString(), refund);

                return ConvertToDto(refund);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming refund request {RefundRequestId}", refundRequestId);
                throw;
            }
        }
        private static RefundRequestDto ConvertToDto(Domain.Entities.RefundRequest refundRequest)
        {
            return new RefundRequestDto
            {
                Id = refundRequest.Id,
                OrderId = refundRequest.OrderId,
                TrackingCode = refundRequest.TrackingCode,
                RequestedByUserId = refundRequest.RequestedByUserId,
                RequestedAt = refundRequest.RequestedAt,
                Status = refundRequest.Status,
                ProcessedByUserId = refundRequest.ProcessedByUserId,
                ProcessedAt = refundRequest.ProcessedAt,
                RefundAmount = refundRequest.RefundAmount,
                ShippingFee = refundRequest.ShippingFee,
                TotalAmount = refundRequest.TotalAmount,
                CreatedAt = refundRequest.CreatedAt,
                CreatedBy = refundRequest.CreatedBy,
                LastModifiedAt = refundRequest.LastModifiedAt,
                LastModifiedBy = refundRequest.LastModifiedBy,
                RefundDetails = refundRequest.RefundDetails.Select(rd => new RefundDetailDto
                {
                    Id = rd.Id,
                    OrderItemId = rd.OrderItemId,
                    RefundRequestId = rd.RefundRequestId,
                    Reason = rd.Reason,
                    ImageUrl = rd.ImageUrl,
                    UnitPrice = rd.UnitPrice,
                    CreatedAt = rd.CreatedAt,
                    CreatedBy = rd.CreatedBy
                }).ToList()
            };
        }
    }
}