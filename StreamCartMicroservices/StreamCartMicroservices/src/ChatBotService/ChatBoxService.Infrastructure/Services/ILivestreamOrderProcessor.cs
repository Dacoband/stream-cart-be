using ChatBoxService.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace ChatBoxService.Infrastructure.Services
{
    public interface ILivestreamOrderProcessor
    {
        Task<OrderProcessingResult> ProcessLivestreamOrderAsync(string message, Guid livestreamId, Guid userId);
    }

    public class LivestreamOrderProcessor : ILivestreamOrderProcessor
    {
        private readonly IOrderServiceClient _orderService;
        private readonly ILivestreamServiceClient _livestreamService;
        private readonly ILogger<LivestreamOrderProcessor> _logger;

        public LivestreamOrderProcessor(
            IOrderServiceClient orderService,
            ILivestreamServiceClient livestreamService,
            ILogger<LivestreamOrderProcessor> logger)
        {
            _orderService = orderService;
            _livestreamService = livestreamService;
            _logger = logger;
        }

        public async Task<OrderProcessingResult> ProcessLivestreamOrderAsync(string message, Guid livestreamId, Guid userId)
        {
            try
            {
                // 🤖 1. Phân tích tin nhắn để extract SKU và quantity
                var orderIntent = AnalyzeOrderMessage(message);

                if (!orderIntent.IsOrderIntent)
                {
                    return new OrderProcessingResult
                    {
                        Success = false,
                        Message = "Tin nhắn không chứa ý định đặt hàng rõ ràng. Vui lòng sử dụng format: 'Đặt [SKU] x[số lượng]'"
                    };
                }

                // 📦 2. Tìm sản phẩm theo SKU trong livestream
                var product = await _livestreamService.GetProductBySkuAsync(livestreamId, orderIntent.SKU);
                if (product == null)
                {
                    return new OrderProcessingResult
                    {
                        Success = false,
                        Message = $"❌ Không tìm thấy sản phẩm với mã '{orderIntent.SKU}' trong livestream này."
                    };
                }

                // 📊 3. Kiểm tra tồn kho
                if (product.ProductStock < orderIntent.Quantity)
                {
                    return new OrderProcessingResult
                    {
                        Success = false,
                        Message = $"❌ Sản phẩm '{product.ProductName}' chỉ còn {product.ProductStock} sản phẩm. Bạn đặt {orderIntent.Quantity} sản phẩm."
                    };
                }

                // 💾 4. Tạo StreamEvent
                var streamEvent = await _livestreamService.CreateStreamEventAsync(livestreamId, userId, message, product.Id);

                // 📉 5. Reserve stock
                var newStock = product.Stock - orderIntent.Quantity;
                var stockReserved = await _livestreamService.UpdateProductStockAsync(
                     livestreamId,           // Guid livestreamId
                     product.ProductId,      // string productId
                     product.VariantId,      // string? variantId
                     newStock,               // int newStock
                     userId.ToString()       // string modifiedBy
                 );
                if (!stockReserved)
                {
                    return new OrderProcessingResult
                    {
                        Success = false,
                        Message = "❌ Không thể cập nhật tồn kho. Vui lòng thử lại."
                    };
                }

                // 🛒 6. Tạo order
                var orderRequest = CreateOrderRequest(livestreamId, userId, product, orderIntent.Quantity, streamEvent.Id);
                var orderResult = await _orderService.CreateMultiOrderAsync(orderRequest);

                if (!orderResult.Success)
                {
                    // Rollback stock
                    var rollbackStock = product.Stock; // Trả về stock ban đầu
                    await _livestreamService.UpdateProductStockAsync(
                        livestreamId,           // Guid livestreamId
                        product.ProductId,      // string productId
                        product.VariantId,      // string? variantId
                        rollbackStock,          // int newStock
                        "rollback"              // string modifiedBy
                    );
                    return new OrderProcessingResult
                    {
                        Success = false,
                        Message = "❌ Không thể tạo đơn hàng. Vui lòng thử lại."
                    };
                }

                // ⏰ 7. Schedule auto-cancel
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(10));
                    await AutoCancelUnpaidOrderAsync(orderResult.OrderId!.Value, product, orderIntent.Quantity, livestreamId);
                });

                return new OrderProcessingResult
                {
                    Success = true,
                    Message = $"✅ Đặt hàng thành công!\n📦 {product.ProductName}\n🔢 Số lượng: {orderIntent.Quantity}\n💰 Tổng tiền: {product.Price * orderIntent.Quantity:N0} VND\n⏰ Thanh toán trong 10 phút.",
                    OrderId = orderResult.OrderId,
                    Product = product,
                    OrderIntent = orderIntent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing livestream order");
                return new OrderProcessingResult
                {
                    Success = false,
                    Message = "❌ Có lỗi hệ thống. Vui lòng thử lại sau."
                };
            }
        }

        private OrderIntent AnalyzeOrderMessage(string message)
        {
            var lowerMessage = message.ToLower().Trim();

            // Regex patterns for order detection
            var orderPatterns = new[]
            {
                @"đặt\s+([a-zA-Z0-9\-_]+)\s*x?(\d+)?", // đặt ABC123 x2
                @"mua\s+(\d+)?\s*[^\s]*\s*([a-zA-Z0-9\-_]+)", // mua 2 cái ABC123
                @"order\s+([a-zA-Z0-9\-_]+)\s*qty?\s*(\d+)?", // order ABC123 qty 2
                @"([a-zA-Z0-9\-_]+)\s*x\s*(\d+)", // ABC123 x 2
                @"sku:?\s*([a-zA-Z0-9\-_]+)\s*(\d+)?", // SKU: ABC123 2
            };

            foreach (var pattern in orderPatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(lowerMessage, pattern);
                if (match.Success)
                {
                    var sku = match.Groups[1].Value;
                    var quantityStr = match.Groups[2].Value;
                    var quantity = string.IsNullOrEmpty(quantityStr) ? 1 : int.Parse(quantityStr);

                    return new OrderIntent
                    {
                        IsOrderIntent = true,
                        SKU = sku.ToUpper(),
                        Quantity = quantity,
                        OriginalMessage = message
                    };
                }
            }

            return new OrderIntent
            {
                IsOrderIntent = false,
                OriginalMessage = message
            };
        }

        private CreateMultiOrderRequest CreateOrderRequest(Guid livestreamId, Guid userId, LivestreamProductDTO product, int quantity, Guid streamEventId)
        {
            return new CreateMultiOrderRequest
            {
                AccountId = userId,
                LivestreamId = livestreamId,
                CreatedFromCommentId = streamEventId,
                PaymentMethod = "COD",
                AddressId = (Guid.NewGuid()).ToString(), // Default address - should be retrieved from user service
                OrdersByShop = new List<CreateOrderByShopDto>
                {
                    new CreateOrderByShopDto
                    {
                        ShopId = product.ShopId,
                        Items = new List<CreateOrderItemDto>
                        {
                            new CreateOrderItemDto
                            {
                                ProductId = Guid.Parse(product.ProductId),
                                VariantId = string.IsNullOrEmpty(product.VariantId) ? null : Guid.Parse(product.VariantId),
                                Quantity = quantity
                            }
                        },
                        ShippingFee = 0m,
                        ShippingProviderId = Guid.NewGuid(), // Default shipping
                        CustomerNotes = $"Đặt hàng từ livestream - SKU: {product.SKU}"
                    }
                }
            };
        }

        private async Task AutoCancelUnpaidOrderAsync(Guid orderId, LivestreamProductDTO product, int quantity, Guid livestreamId)
        {
            try
            {
                var paymentStatus = await _orderService.GetOrderPaymentStatusAsync(orderId);
                if (paymentStatus == "Pending")
                {
                    await _orderService.CancelOrderAsync(orderId, "Hủy tự động do không thanh toán trong 10 phút");
                    var restoreStock = product.Stock + quantity; // Trả về stock ban đầu
                    await _livestreamService.UpdateProductStockAsync(
                        livestreamId,           // Guid livestreamId
                        product.ProductId,      // string productId
                        product.VariantId,      // string? variantId
                        restoreStock,           // int newStock
                        "auto-restore"          // string modifiedBy
                    );

                    _logger.LogInformation("Auto-cancelled unpaid order {OrderId} and restored stock", orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto-cancel order process for order {OrderId}", orderId);
            }
        }
    }

    public class OrderIntent
    {
        public bool IsOrderIntent { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string OriginalMessage { get; set; } = string.Empty;
    }

    public class OrderProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public LivestreamProductDTO? Product { get; set; }
        public OrderIntent? OrderIntent { get; set; }
    }
}