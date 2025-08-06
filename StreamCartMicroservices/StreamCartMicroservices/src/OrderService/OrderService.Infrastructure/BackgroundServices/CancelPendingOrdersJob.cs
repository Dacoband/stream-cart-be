using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Domain.Enums;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.BackgroundServices
{
    public class CancelPendingOrdersJob : IJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CancelPendingOrdersJob> _logger;

        public CancelPendingOrdersJob(IOrderRepository repo, ILogger<CancelPendingOrdersJob> logger)
        {
            _orderRepository = repo;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cutoff = DateTime.UtcNow.AddDays(-2);
            var orders = await _orderRepository.GetOrdersByStatusAndCreatedBeforeAsync(OrderStatus.Pending, cutoff);

            foreach (var order in orders)
            {
                order.UpdateStatus(OrderStatus.Cancelled, "System");
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                _logger.LogInformation("Tự động hủy đơn Pending {Id}", order.Id);
            }
        }
    }

}
