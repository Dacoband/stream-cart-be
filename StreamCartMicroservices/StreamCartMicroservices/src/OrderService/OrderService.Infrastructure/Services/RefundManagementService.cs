using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.RefundCommands;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
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
                   // ShippingFee = createRefundDto.ShippingFee,
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
                    ModifiedBy = updateStatusDto.ModifiedBy,
                    TrackingCode = updateStatusDto.TrackingCode
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund status for refund {RefundRequestId}", updateStatusDto.RefundRequestId);
                throw;
            }
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
    }
}