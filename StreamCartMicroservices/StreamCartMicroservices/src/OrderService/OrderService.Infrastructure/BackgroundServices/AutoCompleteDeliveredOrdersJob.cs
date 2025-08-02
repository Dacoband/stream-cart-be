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
    public class AutoCompleteDeliveredOrdersJob : IJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<AutoCompleteDeliveredOrdersJob> _logger;

        public AutoCompleteDeliveredOrdersJob(IOrderRepository repo, ILogger<AutoCompleteDeliveredOrdersJob> logger)
        {
            _orderRepository = repo;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cutoff = DateTime.UtcNow.AddDays(-3);
            var orders = await _orderRepository.GetOrdersByStatusAndModifiedBeforeAsync(OrderStatus.Delivered, cutoff);

            foreach (var order in orders)
            {
                order.UpdateStatus(OrderStatus.Completed, "System");
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                _logger.LogInformation("Tự động hoàn tất đơn Delivered {Id}", order.Id);
            }
        }
    }

}
