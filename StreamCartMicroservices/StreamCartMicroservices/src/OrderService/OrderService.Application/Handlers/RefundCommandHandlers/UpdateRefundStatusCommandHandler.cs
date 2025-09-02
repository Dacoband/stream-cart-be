using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.RefundCommands;
using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Handlers.RefundCommandHandlers
{
    public class UpdateRefundStatusCommandHandler : IRequestHandler<UpdateRefundStatusCommand, RefundRequestDto>
    {
        private readonly IRefundRequestRepository _refundRequestRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IDeliveryClient _deliveryClient;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<UpdateRefundStatusCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateRefundStatusCommandHandler(
            IRefundRequestRepository refundRequestRepository,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IDeliveryClient deliveryClient,
            IProductServiceClient productServiceClient,
            ILogger<UpdateRefundStatusCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _refundRequestRepository = refundRequestRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _deliveryClient = deliveryClient;
            _productServiceClient = productServiceClient;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<RefundRequestDto> Handle(UpdateRefundStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                _logger.LogInformation("Updating refund status for refund {RefundRequestId} to {NewStatus}",
                    request.RefundRequestId, request.NewStatus);

                // Get refund request
                var refundRequest = await _refundRequestRepository.GetWithDetailsAsync(request.RefundRequestId);
                if (refundRequest == null)
                    throw new ApplicationException($"Refund request with ID {request.RefundRequestId} not found");

                // Get original order for address information
                var order = await _orderRepository.GetByIdAsync(refundRequest.OrderId.ToString());
                if (order == null)
                    throw new ApplicationException($"Order with ID {refundRequest.OrderId} not found");

                // Handle status-specific logic
                switch (request.NewStatus)
                {
                    case RefundStatus.Packed:
                        await CreateReturnShipmentAsync(refundRequest, order);
                        break;

                    case RefundStatus.OnDelivery:
                    case RefundStatus.Delivered:
                    case RefundStatus.Completed:
                    case RefundStatus.Refunded:
                        break;
                }

                // Update status
                refundRequest.UpdateStatus(request.NewStatus, userId.ToString());

                // Save changes
                await _refundRequestRepository.ReplaceAsync(refundRequest.Id.ToString(), refundRequest);

                _logger.LogInformation("Refund status updated successfully for refund {RefundRequestId}", request.RefundRequestId);

                // ✅ Manually convert to DTO without AutoMapper
                return ConvertToRefundRequestDto(refundRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating refund status for refund {RefundRequestId}", request.RefundRequestId);
                throw;
            }
        }

        private async Task CreateReturnShipmentAsync(RefundRequest refundRequest, Orders order)
        {
            try
            {
                // ✅ Get order items being refunded và tạo delivery items từ thông tin thực tế
                var refundItemIds = refundRequest.RefundDetails.Select(rd => rd.OrderItemId).ToList();
                var orderItems = await _orderItemRepository.GetByOrderIdAsync(order.Id);
                var refundedOrderItems = orderItems.Where(oi => refundItemIds.Contains(oi.Id)).ToList();

                var deliveryItemList = new List<UserOrderItem>();

                foreach (var item in refundedOrderItems)
                {
                    var product = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Product {ProductId} not found for refund item", item.ProductId);
                        continue;
                    }

                    var deliveryItem = new UserOrderItem
                    {
                        Quantity = item.Quantity,
                        Name = product.ProductName
                    };

                    if (item.VariantId.HasValue)
                    {
                        var variant = await _productServiceClient.GetVariantByIdAsync(item.VariantId.Value);
                        if (variant != null)
                        {
                            // ✅ Nếu có Variant → dùng Variant dimensions
                            deliveryItem.Width = Math.Max(1, (int)(variant.Width ?? 1));
                            deliveryItem.Weight = Math.Max(1, (int)(variant.Weight ?? 1));
                            deliveryItem.Height = Math.Max(1, (int)(variant.Height ?? 1));
                            deliveryItem.Length = Math.Max(1, (int)(variant.Length ?? 1));
                        }
                        else
                        {
                            // ✅ Fallback to product dimensions if variant not found
                            deliveryItem.Width = Math.Max(1, (int)(product.Width ?? 1));
                            deliveryItem.Weight = Math.Max(1, (int)(product.Weight ?? 1));
                            deliveryItem.Height = Math.Max(1, (int)(product.Height ?? 1));
                            deliveryItem.Length = Math.Max(1, (int)(product.Length ?? 1));
                        }
                    }
                    else
                    {
                        // ✅ Nếu không có Variant → dùng Product dimensions
                        deliveryItem.Width = Math.Max(1, (int)(product.Width ?? 1));
                        deliveryItem.Weight = Math.Max(1, (int)(product.Weight ?? 1));
                        deliveryItem.Height = Math.Max(1, (int)(product.Height ?? 1));
                        deliveryItem.Length = Math.Max(1, (int)(product.Length ?? 1));
                    }

                    deliveryItemList.Add(deliveryItem);
                }

                // Create return shipment from customer back to shop
                var returnShipmentRequest = new UserCreateOrderRequest
                {
                    // Customer address as sender (return from customer)
                    FromName = order.ToName,
                    FromPhone = order.ToPhone,
                    FromAddress = order.ToAddress,
                    FromWard = order.ToWard,
                    FromDistrict = order.ToDistrict,
                    FromProvince = order.ToProvince,

                    // Shop address as receiver
                    ToName = order.FromShop,
                    ToPhone = order.FromPhone,
                    ToAddress = order.FromAddress,
                    ToWard = order.FromWard,
                    ToDistrict = order.FromDistrict,
                    ToProvince = order.FromProvince,

                    ServiceTypeId = 2,
                    Note = $"Trả hàng cho đơn hàng #{order.OrderCode}",
                    Description = $"Trả hàng refund #{refundRequest.Id}",
                    CodAmount = 0, // No COD for return shipment
                    Items = deliveryItemList
                };

                var ghnResponse = await _deliveryClient.CreateGhnOrderAsync(returnShipmentRequest);
                if (ghnResponse.Success)
                {
                    refundRequest.SetTrackingCode(ghnResponse.Data.DeliveryId, "system");
                    _logger.LogInformation("Return shipment created for refund {RefundRequestId} with tracking {TrackingCode}",
                        refundRequest.Id, ghnResponse.Data.DeliveryId);
                }
                else
                {
                    _logger.LogWarning("Failed to create return shipment for refund {RefundRequestId}", refundRequest.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating return shipment for refund {RefundRequestId}", refundRequest.Id);
                // Don't throw exception here, just log the error
            }
        }
        private static RefundRequestDto ConvertToRefundRequestDto(RefundRequest refundRequest)
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