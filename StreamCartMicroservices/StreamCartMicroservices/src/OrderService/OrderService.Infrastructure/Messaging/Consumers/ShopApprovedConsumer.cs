using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Entities;
using Shared.Messaging.Consumers;
using ShopService.Application.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class ShopApprovedConsumer : IConsumer<ShopApproved>, IBaseConsumer
    {
        private readonly ILogger<ShopApprovedConsumer> _logger;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Creates a new instance of ShopApprovedConsumer
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="shopServiceClient">Shop service client</param>
        /// <param name="orderRepository">Order repository</param>
        public ShopApprovedConsumer(
            ILogger<ShopApprovedConsumer> logger,
            IShopServiceClient shopServiceClient,
            IOrderRepository orderRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _shopServiceClient = shopServiceClient ?? throw new ArgumentNullException(nameof(shopServiceClient));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        /// <summary>
        /// Consumes the ShopApproved event
        /// </summary>
        /// <param name="context">Consumer context</param>
        public async Task Consume(ConsumeContext<ShopApproved> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Shop approved: {ShopId}, Name: {ShopName}, Account: {AccountId}, ApprovalDate: {ApprovalDate}",
                message.ShopId,
                message.ShopName,
                message.AccountId,
                message.ApprovalDate);

            try
            {
                // Get all pending orders for the shop
                var pendingOrders = await _orderRepository.GetByShopIdAsync(message.ShopId);
                var pendingOrdersCount = pendingOrders.Count();
                
                _logger.LogInformation("Found {OrderCount} pending orders for shop {ShopId}", 
                    pendingOrdersCount, message.ShopId);
                    
                // Update local cached shop data if needed
                // This is just a placeholder for how you might update local shop data in OrderService
                
                _logger.LogInformation("Successfully processed ShopApproved event for {ShopId}", message.ShopId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ShopApproved event for ShopId: {ShopId}", message.ShopId);
            }
        }
    }
}