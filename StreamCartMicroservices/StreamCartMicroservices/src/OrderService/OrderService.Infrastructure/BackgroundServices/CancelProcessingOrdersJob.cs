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
    public class CancelProcessingOrdersJob : IJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CancelProcessingOrdersJob> _logger;

        public CancelProcessingOrdersJob(IOrderRepository repo, ILogger<CancelProcessingOrdersJob> logger)
        {
            _orderRepository = repo;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cutoff = DateTime.UtcNow.AddDays(-1);
            var orders = await _orderRepository.GetOrdersByStatusAndModifiedBeforeAsync(OrderStatus.Processing, cutoff);

            foreach (var order in orders)   
            {
                order.UpdateStatus(OrderStatus.Cancelled, "System");
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                _logger.LogInformation("Tự động hủy đơn Processing {Id}", order.Id);
            }
        }
    }

}
