using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Queries.OrderQueries;
using OrderService.Domain.Enums;

namespace OrderService.Application.Handlers.OrderQueryHandlers
{
    public class GetOrderStatisticsQueryHandler : IRequestHandler<GetOrderStatisticsQuery, OrderStatisticsDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GetOrderStatisticsQueryHandler> _logger;

        public GetOrderStatisticsQueryHandler(
            IOrderRepository orderRepository,
            ILogger<GetOrderStatisticsQueryHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderStatisticsDto> Handle(GetOrderStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting order statistics for shop {ShopId} between {StartDate} and {EndDate}",
                    request.ShopId, request.StartDate, request.EndDate);

                var statistics = await _orderRepository.GetOrderStatisticsAsync(
                    request.ShopId,
                    request.StartDate,
                    request.EndDate);

                if (statistics == null)
                {
                    _logger.LogInformation("No statistics found for shop {ShopId} in the specified date range", request.ShopId);
                    return new OrderStatisticsDto
                    {
                        ShopId = request.ShopId,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        OrdersByStatus = new Dictionary<OrderStatus, int>(),
                        TotalOrders = 0,
                        TotalRevenue = 0,
                        TotalCommissionFees = 0,
                        NetRevenue = 0,
                        AverageOrderValue = 0,
                        TotalItemsSold = 0
                    };
                }
                var statisticsDto = new OrderStatisticsDto
                {
                    ShopId = request.ShopId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalOrders = statistics.TotalOrders,
                    TotalRevenue = statistics.TotalRevenue,
                    TotalCommissionFees = statistics.TotalCommissionFees,
                    NetRevenue = statistics.NetRevenue,
                    AverageOrderValue = statistics.AverageOrderValue,
                    TotalItemsSold = statistics.TotalItemsSold,
                    OrdersByStatus = statistics.OrdersByStatus
                };

                return statisticsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics for shop {ShopId}: {ErrorMessage}", request.ShopId, ex.Message);
                throw;
            }
        }
    }
}