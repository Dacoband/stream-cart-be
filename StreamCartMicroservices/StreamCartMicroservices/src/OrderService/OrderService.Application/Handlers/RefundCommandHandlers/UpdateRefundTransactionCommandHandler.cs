using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.RefundCommands;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Application.Interfaces.IRepositories;
using Shared.Common.Services.User;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Handlers.RefundCommandHandlers
{
    public class UpdateRefundTransactionCommandHandler : IRequestHandler<UpdateRefundTransactionCommand, RefundRequestDto>
    {
        private readonly IRefundRequestRepository _refundRequestRepository;
        private readonly ILogger<UpdateRefundTransactionCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateRefundTransactionCommandHandler(
            IRefundRequestRepository refundRequestRepository,
            ILogger<UpdateRefundTransactionCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _refundRequestRepository = refundRequestRepository;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<RefundRequestDto> Handle(UpdateRefundTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                _logger.LogInformation("Updating transaction ID for refund {RefundRequestId} to {TransactionId}",
                    request.RefundRequestId, request.TransactionId);

                // Get refund request
                var refundRequest = await _refundRequestRepository.GetWithDetailsAsync(request.RefundRequestId);
                if (refundRequest == null)
                    throw new ApplicationException($"Refund request with ID {request.RefundRequestId} not found");

                // Update transaction ID
                refundRequest.UpdateTransactionId(request.TransactionId, userId.ToString());

                // Save changes
                await _refundRequestRepository.ReplaceAsync(refundRequest.Id.ToString(), refundRequest);

                _logger.LogInformation("Transaction ID updated successfully for refund {RefundRequestId}", request.RefundRequestId);

                // Convert to DTO
                return ConvertToRefundRequestDto(refundRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction ID for refund {RefundRequestId}", request.RefundRequestId);
                throw;
            }
        }

        private static RefundRequestDto ConvertToRefundRequestDto(Domain.Entities.RefundRequest refundRequest)
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
                BankName = refundRequest.BankName,
                BankNumber = refundRequest.BankNumber,
                TransactionId = refundRequest.TransactionId,
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