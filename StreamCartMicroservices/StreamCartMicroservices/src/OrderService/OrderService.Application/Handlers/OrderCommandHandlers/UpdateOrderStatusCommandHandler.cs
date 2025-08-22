using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Enums;
using System.Collections.Generic;
using Shared.Messaging.Event.OrderEvents;
using MassTransit;
using OrderService.Application.Interfaces;
using OrderService.Application.DTOs.Delivery;
using OrderService.Domain.Entities;
using OrderService.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Application.Handlers.OrderCommandHandlers
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IDeliveryClient _deliveryClient;
        private readonly IOrderService _orderService; 
        private readonly IProductServiceClient _productServiceClient;

        public UpdateOrderStatusCommandHandler(
            IOrderRepository orderRepository,
            ILogger<UpdateOrderStatusCommandHandler> logger,
            IPublishEndpoint publishEndpoint, IAccountServiceClient accountServiceClient, IDeliveryClient deliveryClient, IProductServiceClient productServiceClient, IOrderService orderService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
            _accountServiceClient = accountServiceClient;
            _deliveryClient = deliveryClient;
            _productServiceClient = productServiceClient;
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            //try
            //{
            //    _logger.LogInformation("Updating order status for order {OrderId} to {NewStatus}", request.OrderId, request.NewStatus);

            //    // Get the order
            //    var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
            //    var deliveryItemList = new List<UserOrderItem>();
            //    foreach (var item in order.Items)
            //    {
            //        var deliveryItem = new UserOrderItem();
            //        var product = await _productServiceClient.GetProductByIdAsync(item.ProductId);
            //        deliveryItem.Quantity = item.Quantity;
            //        deliveryItem.Name = product.ProductName;
            //        deliveryItem.Width = (int)product.Width;
            //        deliveryItem.Weight = (int)product.Weight;
            //        deliveryItem.Height = (int)product.Height;
            //        deliveryItem.Length = (int)product.Length;
            //        if (product == null)
            //            throw new ApplicationException($"Không tìm thấy sản phẩm {item.Id}");
            //        if (item.VariantId.HasValue)
            //        {
            //            var variant = await _productServiceClient.GetVariantByIdAsync(item.VariantId.Value);
            //            if (variant == null)
            //                throw new ApplicationException($"Không tìm thấy sản phẩm {item.VariantId}");

            //            deliveryItem.Name = product.ProductName;
            //            deliveryItem.Width = (int)(product.Width ?? variant.Width);
            //            deliveryItem.Weight = (int)(product.Weight ?? variant.Weight);
            //            deliveryItem.Height = (int)(product.Height ?? variant.Height);
            //            deliveryItem.Length = (int)(product.Length ?? variant.Length);

            //        }
            //        deliveryItemList.Add(deliveryItem);
            //    }
            //    if (order == null)
            //    {
            //        _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
            //        throw new ApplicationException($"Order with ID {request.OrderId} not found");
            //    }
            //    var message = "";

            //    switch (request.NewStatus)
            //    {
            //        case OrderStatus.Waiting:
            //            order.UpdateStatus(OrderStatus.Waiting, request.ModifiedBy);
            //            message = "Đơn hàng đang được khởi tạo";
            //            break;

            //        case OrderStatus.Pending:
            //            order.UpdateStatus(OrderStatus.Pending, request.ModifiedBy);
            //            message = "Chờ người bán xác nhận đơn hàng";
            //            break;

            //        case OrderStatus.Processing:
            //            order.UpdateStatus(OrderStatus.Processing, request.ModifiedBy);
            //            message = "Người gửi đang chuẩn bị hàng";

            //            // 👉 Gọi hàm tạo đơn GHN
            //            var ghnRequest = new UserCreateOrderRequest
            //            {
            //                FromName = order.FromShop,
            //                FromPhone = order.FromPhone,
            //                FromProvince = order.FromProvince,
            //                FromDistrict = order.FromDistrict,
            //                FromWard = order.FromWard,
            //                FromAddress = order.FromAddress,

            //                ToName = order.ToName,
            //                ToPhone = order.ToPhone,
            //                ToProvince = order.ToProvince,
            //                ToDistrict = order.ToDistrict,
            //                ToWard = order.ToWard,
            //                ToAddress = order.ToAddress,

            //                ServiceTypeId = 2, 
            //                Note = order.CustomerNotes,
            //                Description = $"Đơn hàng #{order.OrderCode}",
            //                CodAmount = (int?)order.FinalAmount,
            //                Items = deliveryItemList,

            //            };
            //            if((order.PaymentStatus == PaymentStatus.paid))
            //            {
            //                ghnRequest.CodAmount = 0;
            //            }
            //            var ghnResponse = await _deliveryClient.CreateGhnOrderAsync(ghnRequest);
            //            if (!ghnResponse.Success)
            //            {
            //                _logger.LogWarning("Không thể tạo đơn GHN cho đơn hàng {OrderId}", order.Id);
            //                throw new ApplicationException("Tạo đơn giao hàng thất bại: " + ghnResponse.Message);
            //            }

            //            break;

            //        case OrderStatus.Packed:
            //            order.UpdateStatus(OrderStatus.Packed, request.ModifiedBy);
            //            message = "Đơn hàng đã được đóng gói";
            //            break;

            //        case OrderStatus.OnDelivere:
            //            order.UpdateStatus(OrderStatus.OnDelivere, request.ModifiedBy);
            //            message = "Đơn hàng đang được vận chuyển";
            //            break;

            //        case OrderStatus.Delivered:
            //            order.UpdateStatus(OrderStatus.Delivered, request.ModifiedBy);
            //            message = "Đơn hàng đã được giao thành công";
            //            break;

            //        case OrderStatus.Completed:
            //            order.UpdateStatus(OrderStatus.Completed, request.ModifiedBy);
            //            message = "Đơn hàng đã hoàn tất";
            //            break;

            //        case OrderStatus.Returning:
            //            order.UpdateStatus(OrderStatus.Returning, request.ModifiedBy);
            //            message = "Khách hàng đã yêu cầu trả hàng";
            //            break;

            //        case OrderStatus.Refunded:
            //            order.UpdateStatus(OrderStatus.Refunded, request.ModifiedBy);
            //            message = "Đơn hàng đã được hoàn tiền";
            //            break;

            //        case OrderStatus.Cancelled:
            //            order.UpdateStatus(OrderStatus.Cancelled, request.ModifiedBy);
            //            message = "Đơn hàng đã bị hủy";
            //            break;

            //        default:
            //            _logger.LogWarning("Chuyển trạng thái không được hỗ trợ: {NewStatus}", request.NewStatus);
            //            throw new InvalidOperationException($"Không hỗ trợ chuyển sang trạng thái: {request.NewStatus}");
            //    }

            //    await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

            //    var shippingAddressDto = new ShippingAddressDto
            //    {
            //        FullName = order.ToName,
            //        Phone = order.ToPhone,
            //        AddressLine1 = order.ToAddress,
            //        Ward = order.ToWard,
            //        City = order.ToProvince,
            //        State = order.ToDistrict,
            //        PostalCode = order.ToPostalCode,
            //        Country = "Vietnam", 
            //        IsDefault = false
            //    };

            //    // Convert items to DTOs
            //    var orderItemDtos = new List<OrderItemDto>();
            //    if (order.Items != null)
            //    {
            //        foreach (var item in order.Items)
            //        {
            //            orderItemDtos.Add(new OrderItemDto
            //            {
            //                Id = item.Id,
            //                OrderId = item.OrderId,
            //                ProductId = item.ProductId,
            //                VariantId = item.VariantId,
            //                Quantity = item.Quantity,
            //                UnitPrice = item.UnitPrice,
            //                DiscountAmount = item.DiscountAmount,
            //                TotalPrice = item.TotalPrice,
            //                Notes = item.Notes
            //            });
            //        }
            //    }

            //    var orderDto = new OrderDto
            //    {
            //        Id = order.Id,
            //        OrderCode = order.OrderCode,
            //        AccountId = order.AccountId,
            //        ShopId = order.ShopId,
            //        OrderDate = order.OrderDate,
            //        OrderStatus = order.OrderStatus,
            //        PaymentStatus = order.PaymentStatus,
            //        ShippingAddress = shippingAddressDto,
            //        ShippingProviderId = order.ShippingProviderId,
            //        ShippingFee = order.ShippingFee,
            //        TotalPrice = order.TotalPrice,
            //        DiscountAmount = order.DiscountAmount,
            //        FinalAmount = order.FinalAmount,
            //        CustomerNotes = order.CustomerNotes,
            //        TrackingCode = order.TrackingCode,
            //        EstimatedDeliveryDate = order.EstimatedDeliveryDate,
            //        ActualDeliveryDate = order.ActualDeliveryDate,
            //        LivestreamId = order.LivestreamId,
            //        Items = orderItemDtos
            //    };

            //    _logger.LogInformation("Order status updated successfully for order {OrderId}", request.OrderId);

            //    var recipent = new List<string> { request.ModifiedBy };


            //    var shopAccount = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
            //    foreach (var acc in shopAccount)
            //    {
            //        recipent.Add(acc.Id.ToString());
            //    }
            //    var orderChangEvent = new OrderCreatedOrUpdatedEvent()
            //    {
            //        OrderCode = order.OrderCode,
            //        Message = message,
            //        UserId = recipent,
            //        OrderStatus = request.NewStatus.ToString(),
            //    };
            //    if(orderChangEvent.OrderStatus == "Pending")
            //    {
            //        orderChangEvent.OrderStatus = null;
            //    }
            //    await _publishEndpoint.Publish(orderChangEvent);

            //    return orderDto;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error updating order status: {ErrorMessage}", ex.Message);
            //    throw;
            //}
            return await _orderService.UpdateOrderStatus(new UpdateOrderStatusDto()
            {
                ModifiedBy = request.ModifiedBy,
                NewStatus = request.NewStatus,
                OrderId = request.OrderId,
            });
        }
    }
}