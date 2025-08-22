using AutoMapper;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.DTOs.WalletDTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderQueries;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Repositories;
using Shared.Common.Domain.Bases;
using Shared.Common.Services.User;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Services
{
    public class OrderManagementService : IOrderService
    {
        private readonly IMediator _mediator;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderManagementService> _logger;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWalletServiceClient _walletServiceClient;
        private readonly IMapper _mapper;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IDeliveryClient _deliveryClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IOrderItemRepository _orderItemRepository;


        public OrderManagementService(
            IMediator mediator,
            IOrderRepository orderRepository,
            ILogger<OrderManagementService> logger,
            IAccountServiceClient accountServiceClient,
            IShopServiceClient shopServiceClient,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IWalletServiceClient walletServiceClient, IProductServiceClient productServiceClient, IDeliveryClient deliveryClient, IPublishEndpoint publishEndpoint, IOrderItemRepository orderItemRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountServiceClient = accountServiceClient;
            _shopServiceClient = shopServiceClient;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _walletServiceClient = walletServiceClient;
            _productServiceClient = productServiceClient;
            _deliveryClient = deliveryClient;
            _publishEndpoint = publishEndpoint;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                _logger.LogInformation("Creating order for account {AccountId}", accountId);
                // Validate account exists
                var account = await _accountServiceClient.GetAccountByIdAsync(accountId);
                if (account == null)
                    throw new ApplicationException($"Account with ID {accountId} not found");

                // Validate shop exists 
                var shop = await _shopServiceClient.GetShopByIdAsync(createOrderDto.ShopId);
                if (shop == null)
                    throw new ApplicationException($"Shop with ID {createOrderDto.ShopId} not found");

                // Validate relationship (optional, depending on business rules)
                //var isShopMember = await _shopServiceClient.IsShopMemberAsync(
                //    createOrderDto.ShopId, accountId);
                //if (!isShopMember)
                //    throw new ApplicationException("Account is not authorized to create orders for this shop");
                var shippingAddress = createOrderDto.ShippingAddress;

                var command = new CreateOrderCommand
                {
                    AccountId = accountId,
                    ShopId = createOrderDto.ShopId,
                    CustomerName = shippingAddress.FullName ?? string.Empty,
                    CustomerEmail = account?.Email ?? string.Empty,
                    CustomerPhone = shippingAddress.Phone ?? string.Empty,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    PaymentMethod = createOrderDto.PaymentMethod ?? "COD",
                    ShippingMethod = string.Empty,
                    ShippingFee = createOrderDto.ShippingFee,
                    PromoCode = string.Empty,
                    DiscountAmount = 0,
                    Notes = createOrderDto.CustomerNotes,
                    ShippingProviderId = createOrderDto.ShippingProviderId,
                    LivestreamId = createOrderDto.LivestreamId,
                    CreatedFromCommentId = createOrderDto.CreatedFromCommentId,
                    OrderItems = createOrderDto.Items?.Select(i => new CreateOrderItemDto
                    {
                        ProductId = i.ProductId,
                        VariantId = i.VariantId,
                        Quantity = i.Quantity,
                        //Notes = i.Notes
                    }).ToList() ?? new List<CreateOrderItemDto>()

                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Getting order {OrderId}", orderId);
                var query = new GetOrderByIdQuery { OrderId = orderId };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByCodeAsync(string orderCode)
        {
            try
            {
                _logger.LogInformation("Getting order by code {OrderCode}", orderCode);
                var query = new GetOrderByCodeQuery { OrderCode = orderCode };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by code: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResult<OrderDto>> GetOrdersByAccountIdAsync(Guid accountId, FilterOrderDTO filter)
        {
            try
            {
                _logger.LogInformation("Getting orders for account {AccountId}", accountId);

                var searchParams = new OrderSearchParamsDto
                {
                    AccountId = accountId,
                    PageNumber = filter.PageIndex,
                    PageSize = filter.PageSize,
                    OrderStatus = filter.Status
                };

                var query = new SearchOrdersQuery { SearchParams = searchParams };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by account ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResult<OrderDto>> GetOrdersByShopIdAsync(Guid shopId, FilterOrderDTO filter)
        {
            try
            {
                _logger.LogInformation("Getting orders for shop {ShopId}", shopId);
                var query = new GetOrdersByShopIdQuery
                {
                    ShopId = shopId,
                    PageNumber = filter.PageIndex,
                    PageSize = filter.PageSize
                };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by shop ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResult<OrderDto>> SearchOrdersAsync(OrderSearchParamsDto searchParams)
        {
            try
            {
                _logger.LogInformation("Searching orders with parameters");
                var query = new SearchOrdersQuery { SearchParams = searchParams };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Updating order {OrderId} status to {Status}", orderId, newStatus);

                var command = new UpdateOrderStatusCommand
                {
                    OrderId = orderId,
                    NewStatus = newStatus,
                    ModifiedBy = modifiedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus newStatus, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Updating order {OrderId} payment status to {Status}", orderId, newStatus);

                var command = new UpdatePaymentStatusCommand
                {
                    OrderId = orderId,
                    NewStatus = newStatus,
                    ModifiedBy = modifiedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdateTrackingCodeAsync(Guid orderId, string trackingCode, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Updating tracking code for order {OrderId}", orderId);

                // Sửa command để sử dụng đúng thuộc tính
                var command = new UpdateTrackingCodeCommand
                {
                    OrderId = orderId,
                    TrackingCode = trackingCode,
                    ModifiedBy = modifiedBy      // Thay UpdatedBy thành ModifiedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracking code: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdateShippingInfoAsync(
            Guid orderId,
            ShippingAddressDto shippingAddress,
            string shippingMethod = null,
            decimal? shippingFee = null,
            string modifiedBy = null)
        {
            try
            {
                _logger.LogInformation("Updating shipping info for order {OrderId}", orderId);

                var command = new UpdateShippingInfoCommand
                {
                    OrderId = orderId,
                    ShippingAddress = shippingAddress,
                    ShippingMethod = shippingMethod,
                    ShippingFee = shippingFee.HasValue ? (decimal)shippingFee : 0m,
                    ModifiedBy = modifiedBy ?? string.Empty
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping info: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> CancelOrderAsync(Guid orderId, string cancelReason, string cancelledBy)
        {
            try
            {
                _logger.LogInformation("Cancelling order {OrderId}", orderId);
                var command = new CancelOrderCommand
                {
                    OrderId = orderId,
                    CancelReason = cancelReason,
                    CancelledBy = cancelledBy
                };
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(Guid shopId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting order statistics for shop {ShopId}", shopId);
                var query = new GetOrderStatisticsQuery
                {
                    ShopId = shopId,
                    StartDate = startDate,
                    EndDate = endDate
                };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateOrderAsync(CreateOrderDto createOrderDto)

        {
            try
            {
                Guid accountId;
                try
                {
                    accountId = _currentUserService.GetUserId();
                }
                catch (UnauthorizedAccessException)
                {
                    return (false, "Authentication failed: User ID not found or invalid");
                }

                if (createOrderDto == null)
                {
                    return (false, "Order data cannot be null");
                }

                if (accountId == Guid.Empty)
                {
                    return (false, "Account ID is required");
                }

                if (createOrderDto.ShippingAddress == null)
                {
                    return (false, "Shipping address is required");
                }

                if (string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.FullName) ||
                    string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.Phone) ||
                    string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.AddressLine1) ||
                    string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.City))
                {
                    return (false, "Shipping address details are incomplete");
                }

                if (createOrderDto.Items == null || !createOrderDto.Items.Any())
                {
                    return (false, "Order must contain at least one item");
                }

                foreach (var item in createOrderDto.Items)
                {
                    if (item.ProductId == Guid.Empty)
                    {
                        return (false, "Product ID is required for all items");
                    }

                    if (item.Quantity <= 0)
                    {
                        return (false, "Quantity must be greater than zero for all items");
                    }

                    if (0 <= 0)
                    {
                        return (false, "Unit price must be greater than zero for all items");
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating order: {ErrorMessage}", ex.Message);
                return (false, $"Validation error: {ex.Message}");
            }
        }




        /// <summary>
        public async Task<OrderDto> ConfirmOrderDeliveredAsync(Guid orderId, string customerId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy đơn hàng {OrderId}", orderId);
                    return null;
                }

                if (order.AccountId != Guid.Parse(customerId) && customerId.ToString() != "system")
                {
                    _logger.LogWarning("Khách hàng {CustomerId} không có quyền xác nhận đơn hàng {OrderId}", customerId, orderId);
                    return null;
                }

                if (order.OrderStatus != OrderStatus.Shipped)
                {
                    _logger.LogWarning("Đơn hàng {OrderId} không ở trạng thái đã giao, không thể xác nhận", orderId);
                    return null;
                }

                // Cập nhật trạng thái đơn hàng
                order.UpdateStatus(OrderStatus.Completed, customerId.ToString());
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                // Xử lý thanh toán cho shop
                await ProcessPaymentToShopAsync(order);
                //Cập nhật reward

                //Cập nhật rate

                // Chuyển đổi sang DTO
                return _mapper.Map<OrderDto>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác nhận đơn hàng {OrderId}", orderId);
                throw;
            }
        }
        private async Task ProcessPaymentToShopAsync(Orders order)
        {
            // Tính toán số tiền thanh toán cho shop (trừ 10% phí)
            decimal totalAmount = order.NetAmount;


            // Gửi yêu cầu thanh toán đến WalletService
            var paymentRequest = new ShopPaymentRequest
            {
                OrderId = order.Id,
                ShopId = order.ShopId,
                Amount = totalAmount,
                Fee = 0,
                TransactionType = "OrderComplete",
                TransactionReference = order.Id.ToString(),
                Description = $"Thanh toán đơn hàng #{order.OrderCode}"
            };

            await _walletServiceClient.ProcessShopPaymentAsync(paymentRequest);

            //// Cập nhật tỷ lệ hoàn thành đơn hàng của shop
            //await _shopServiceClient.UpdateShopCompletionRateAsync(new UpdateShopCompletionRequest
            //{
            //    ShopId = order.ShopId,
            //    RateChange = 0.5m, // Tăng 0.5% cho mỗi đơn hàng hoàn thành
            //    UpdatedByAccountId = Guid.Parse(_systemAccountId)
            //});
        }

        public async Task<OrderDto> UpdateOrderStatus(UpdateOrderStatusDto request)
        {
            try
            {
                _logger.LogInformation("Updating order status for order {OrderId} to {NewStatus}", request.OrderId, request.NewStatus);

                // Get the order
                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                var orderItems = await _orderItemRepository.GetByOrderIdAsync(order.Id);
                var deliveryItemList = new List<UserOrderItem>();

                foreach (var item in orderItems)
                {
                    var product = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                        throw new ApplicationException($"Không tìm thấy sản phẩm {item.ProductId}");

                    var deliveryItem = new UserOrderItem
                    {
                        Quantity = item.Quantity,
                        Name = product.ProductName
                    };

                    if (item.VariantId.HasValue)
                    {
                        var variant = await _productServiceClient.GetVariantByIdAsync(item.VariantId.Value);
                        if (variant == null)
                            throw new ApplicationException($"Không tìm thấy variant {item.VariantId}");

                        // ✅ Nếu có Variant → dùng Variant
                        deliveryItem.Width = Math.Max(1, (int)(variant.Width ?? 1));
                        deliveryItem.Weight = Math.Max(1, (int)(variant.Weight ?? 1));
                        deliveryItem.Height = Math.Max(1, (int)(variant.Height ?? 1));
                        deliveryItem.Length = Math.Max(1, (int)(variant.Length ?? 1));
                    }
                    else
                    {
                        // ✅ Nếu không có Variant → fallback Product
                        deliveryItem.Width = Math.Max(1, (int)(product.Width ?? 1));
                        deliveryItem.Weight = Math.Max(1, (int)(product.Weight ?? 1));
                        deliveryItem.Height = Math.Max(1, (int)(product.Height ?? 1));
                        deliveryItem.Length = Math.Max(1, (int)(product.Length ?? 1));
                    }

                    deliveryItemList.Add(deliveryItem);
                }
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                    throw new ApplicationException($"Order with ID {request.OrderId} not found");
                }
                var message = "";

                switch (request.NewStatus)
                {
                    case OrderStatus.Waiting:
                        order.UpdateStatus(OrderStatus.Waiting, request.ModifiedBy);
                        message = "Đơn hàng đang được khởi tạo";
                        break;

                    case OrderStatus.Pending:
                        order.UpdateStatus(OrderStatus.Pending, request.ModifiedBy);
                        SetTimeForShopIfExists(order, DateTime.UtcNow.AddHours(24));
                        SchedulePendingThenProcessingThenShippedDeadlines(order.Id);
                        message = "Chờ người bán xác nhận đơn hàng";
                        break;

                    case OrderStatus.Processing:
                        order.UpdateStatus(OrderStatus.Processing, request.ModifiedBy);
                        SetTimeForShopIfExists(order, DateTime.UtcNow.AddHours(24));
                        ScheduleProcessingToShippedDeadline(order.Id);
                        message = "Người gửi đang chuẩn bị hàng";

                        // 👉 Gọi hàm tạo đơn GHN
                        var ghnRequest = new UserCreateOrderRequest
                        {
                            FromName = order.FromShop,
                            FromPhone = order.FromPhone,
                            FromProvince = order.FromProvince,
                            FromDistrict = order.FromDistrict,
                            FromWard = order.FromWard,
                            FromAddress = order.FromAddress,

                            ToName = order.ToName,
                            ToPhone = order.ToPhone,
                            ToProvince = order.ToProvince,
                            ToDistrict = order.ToDistrict,
                            ToWard = order.ToWard,
                            ToAddress = order.ToAddress,

                            ServiceTypeId = 2,
                            Note = order.CustomerNotes,
                            Description = $"Đơn hàng #{order.OrderCode}",
                            CodAmount = (int?)order.FinalAmount,
                            Items = deliveryItemList,

                        };
                        if ((order.PaymentStatus == PaymentStatus.Paid))
                        {
                            ghnRequest.CodAmount = 0;
                        }
                        var ghnResponse = await _deliveryClient.CreateGhnOrderAsync(ghnRequest);
                        if (!ghnResponse.Success)
                        {
                            _logger.LogWarning("Không thể tạo đơn GHN cho đơn hàng {OrderId}", order.Id);
                            throw new ApplicationException("Tạo đơn giao hàng thất bại: " + ghnRequest);
                        }

                        break;

                    case OrderStatus.Packed:
                        order.UpdateStatus(OrderStatus.Packed, request.ModifiedBy);
                        message = "Đơn hàng đã được đóng gói";
                        break;

                    case OrderStatus.OnDelivere:
                        order.UpdateStatus(OrderStatus.OnDelivere, request.ModifiedBy);
                        message = "Đơn hàng đang được vận chuyển";
                        break;

                    case OrderStatus.Delivered:
                        order.UpdateStatus(OrderStatus.Delivered, request.ModifiedBy);
                        message = "Đơn hàng đã được giao thành công";
                        break;

                    case OrderStatus.Completed:
                        order.UpdateStatus(OrderStatus.Completed, request.ModifiedBy);
                        message = "Đơn hàng đã hoàn tất";
                        await ConfirmOrderDeliveredAsync(request.OrderId, request.ModifiedBy);
                        break;

                    case OrderStatus.Returning:
                        order.UpdateStatus(OrderStatus.Returning, request.ModifiedBy);
                        message = "Khách hàng đã yêu cầu trả hàng";
                        break;

                    case OrderStatus.Refunded:
                        order.UpdateStatus(OrderStatus.Refunded, request.ModifiedBy);
                        message = "Đơn hàng đã được hoàn tiền";
                        break;

                    case OrderStatus.Cancelled:
                        order.UpdateStatus(OrderStatus.Cancelled, request.ModifiedBy);
                        message = "Đơn hàng đã bị hủy";
                        break;

                    default:
                        _logger.LogWarning("Chuyển trạng thái không được hỗ trợ: {NewStatus}", request.NewStatus);
                        throw new InvalidOperationException($"Không hỗ trợ chuyển sang trạng thái: {request.NewStatus}");
                }

                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                var shippingAddressDto = new ShippingAddressDto
                {
                    FullName = order.ToName,
                    Phone = order.ToPhone,
                    AddressLine1 = order.ToAddress,
                    Ward = order.ToWard,
                    City = order.ToProvince,
                    State = order.ToDistrict,
                    PostalCode = order.ToPostalCode,
                    Country = "Vietnam",
                    IsDefault = false
                };

                // Convert items to DTOs
                var orderItemDtos = new List<OrderItemDto>();
                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        orderItemDtos.Add(new OrderItemDto
                        {
                            Id = item.Id,
                            OrderId = item.OrderId,
                            ProductId = item.ProductId,
                            VariantId = item.VariantId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            DiscountAmount = item.DiscountAmount,
                            TotalPrice = item.TotalPrice,
                            Notes = item.Notes
                        });
                    }
                }

                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    AccountId = order.AccountId,
                    ShopId = order.ShopId,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    PaymentMethod = order.PaymentMethod,
                    ShippingAddress = shippingAddressDto,
                    ShippingProviderId = order.ShippingProviderId,
                    ShippingFee = order.ShippingFee,
                    TotalPrice = order.TotalPrice,
                    DiscountAmount = order.DiscountAmount,
                    FinalAmount = order.FinalAmount,
                    CustomerNotes = order.CustomerNotes,
                    TrackingCode = order.TrackingCode,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    ActualDeliveryDate = order.ActualDeliveryDate,
                    LivestreamId = order.LivestreamId,
                    TimeForShop = order.TimeForShop,
                    Items = orderItemDtos
                };

                _logger.LogInformation("Order status updated successfully for order {OrderId}", request.OrderId);

                var recipent = new List<string> { request.ModifiedBy };


                var shopAccount = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                foreach (var acc in shopAccount)
                {
                    recipent.Add(acc.Id.ToString());
                }
                var shopRate = await CalculateShopCompletionRate(order.ShopId);
                var userRate = await CalculateUserCompletionRate(Guid.Parse(order.CreatedBy));
                var orderChangEvent = new OrderCreatedOrUpdatedEvent()
                {
                    OrderCode = order.OrderCode,
                    Message = message,
                    UserId = recipent,
                    OrderStatus = request.NewStatus.ToString(),
                    ShopRate = shopRate,
                    UserRate = userRate,
                    CreatedBy = order.CreatedBy,
                    ShopId = order.ShopId.ToString(),

                };
                if (orderChangEvent.OrderStatus == "Pending")
                {
                    orderChangEvent.OrderStatus = null;
                }
                await _publishEndpoint.Publish(orderChangEvent);

                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {ErrorMessage}", ex.Message);
                throw;
            }
        }
        public async Task<double> CalculateUserCompletionRate(Guid userId)
        {
            var orders = await _orderRepository.GetByAccountIdAsync(userId);
            int totalOrders = orders.Count();
            int completedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Completed);

            // Chỉ trừ những đơn bị user hủy
            int canceledByUser = orders.Count(o =>
                o.OrderStatus == OrderStatus.Cancelled && o.LastModifiedBy == userId.ToString()
            );

            int validOrders = totalOrders - canceledByUser;

            if (validOrders == 0) return 0.0;

            return (double)completedOrders / validOrders * 100;
        }

        public async Task<double> CalculateShopCompletionRate(Guid shopId)
        {
            var orders = await _orderRepository.GetByShopIdAsync(shopId);
            int totalOrders = orders.Count();
            int completedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Completed);

            // Đơn bị shop hủy
            int canceledByShop = orders.Count(o =>
                o.OrderStatus == OrderStatus.Cancelled && o.LastModifiedBy == shopId.ToString()
            );

            // Đơn do hệ thống hủy (null hoặc không rõ ai hủy)
            int canceledBySystem = orders.Count(o =>
                o.OrderStatus == OrderStatus.Cancelled && string.IsNullOrEmpty(o.LastModifiedBy)
            );

            // Điểm phạt = 1% cho mỗi đơn bị shop hủy
            int penaltyPoints = 1;

            double rawRate = totalOrders == 0 ? 0.0 : (double)completedOrders / totalOrders * 100;
            double finalRate = Math.Max(0, rawRate - penaltyPoints);

            return finalRate;
        }
    private void SchedulePendingThenProcessingThenShippedDeadlines(Guid orderId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // After 24h from Pending: must reach Processing
                    await Task.Delay(TimeSpan.FromHours(24));
                    var order = await _orderRepository.GetByIdAsync(orderId.ToString());
                    if (order == null) return;

                    if (order.OrderStatus < OrderStatus.Processing && order.OrderStatus != OrderStatus.Cancelled)
                    {
                        order.UpdateStatus(OrderStatus.Cancelled, "system-timeout-24h-pending");
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                        return;
                    }

                    if (order.OrderStatus >= OrderStatus.Processing && order.OrderStatus != OrderStatus.Cancelled)
                    {
                        SetTimeForShopIfExists(order, DateTime.UtcNow.AddHours(24));
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                        // After next 24h from Processing: must reach Shipped
                        await Task.Delay(TimeSpan.FromHours(24));

                        var order2 = await _orderRepository.GetByIdAsync(orderId.ToString());
                        if (order2 == null) return;

                        if (order2.OrderStatus < OrderStatus.Shipped && order2.OrderStatus != OrderStatus.Cancelled)
                        {
                            order2.UpdateStatus(OrderStatus.Cancelled, "system-timeout-24h-processing");
                            await _orderRepository.ReplaceAsync(order2.Id.ToString(), order2);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scheduling deadlines for order {OrderId}", orderId);
                }
            });
        }
        private void ScheduleProcessingToShippedDeadline(Guid orderId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24));
                    var order = await _orderRepository.GetByIdAsync(orderId.ToString());
                    if (order == null) return;

                    if (order.OrderStatus < OrderStatus.Shipped && order.OrderStatus != OrderStatus.Cancelled)
                    {
                        order.UpdateStatus(OrderStatus.Cancelled, "system-timeout-24h-processing");
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scheduling processing->shipped deadline for order {OrderId}", orderId);
                }
            });
        }
        private void SetTimeForShopIfExists(Orders order, DateTime deadlineUtc)
        {
            try
            {
                var prop = order.GetType().GetProperty("TimeForShop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite && (prop.PropertyType == typeof(DateTime?) || prop.PropertyType == typeof(DateTime)))
                {
                    object value = prop.PropertyType == typeof(DateTime) ? deadlineUtc : (DateTime?)deadlineUtc;
                    prop.SetValue(order, value);
                }
            }
            catch
            {
                // ignore if property not present
            }
        }
      }
}