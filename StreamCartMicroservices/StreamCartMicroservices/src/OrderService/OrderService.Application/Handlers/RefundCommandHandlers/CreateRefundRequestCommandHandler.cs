using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.RefundCommands;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using Shared.Common.Services.User;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Handlers.RefundCommandHandlers
{
    public class CreateRefundRequestCommandHandler : IRequestHandler<CreateRefundRequestCommand, RefundRequestDto>
    {
        private readonly IRefundRequestRepository _refundRequestRepository;
        private readonly IRefundDetailRepository _refundDetailRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateRefundRequestCommandHandler> _logger;

        public CreateRefundRequestCommandHandler(
            IRefundRequestRepository refundRequestRepository,
            IRefundDetailRepository refundDetailRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            ICurrentUserService currentUserService,
            ILogger<CreateRefundRequestCommandHandler> logger)
        {
            _refundRequestRepository = refundRequestRepository;
            _refundDetailRepository = refundDetailRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<RefundRequestDto> Handle(CreateRefundRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                _logger.LogInformation("Creating refund request for order {OrderId} by user {UserId}", request.OrderId, userId);

                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                if (order == null)
                    throw new ApplicationException($"Order with ID {request.OrderId} not found");

                if (order.AccountId != userId)
                    throw new UnauthorizedAccessException("You can only create refund requests for your own orders");

                var refundRequest = new RefundRequest(request.OrderId, userId, order.ShippingFee);

                foreach (var refundItem in request.RefundItems)
                {
                    var orderItem = await _orderItemRepository.GetByIdAsync(refundItem.OrderItemId.ToString());
                    if (orderItem == null)
                        throw new ApplicationException($"Order item with ID {refundItem.OrderItemId} not found");

                    if (orderItem.OrderId != request.OrderId)
                        throw new ApplicationException($"Order item {refundItem.OrderItemId} does not belong to order {request.OrderId}");

                    var refundDetail = new RefundDetail(
                        refundItem.OrderItemId,
                        refundRequest.Id,
                        refundItem.Reason,
                        orderItem.TotalPrice, 
                        refundItem.ImageUrl
                    );
                    refundDetail.SetCreator(userId.ToString());

                    refundRequest.AddRefundDetail(refundDetail);
                }

                // Save refund request
                await _refundRequestRepository.InsertAsync(refundRequest);

                // Save refund details
                foreach (var detail in refundRequest.RefundDetails)
                {
                    await _refundDetailRepository.InsertAsync(detail);
                }

                _logger.LogInformation("Refund request created successfully with ID {RefundRequestId}", refundRequest.Id);

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
                _logger.LogError(ex, "Error creating refund request for order {OrderId}", request.OrderId);
                throw;
            }
        }
    }
}