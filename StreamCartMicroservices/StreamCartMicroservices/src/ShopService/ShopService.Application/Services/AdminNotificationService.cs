using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly ILogger<AdminNotificationService> _logger;

        public AdminNotificationService(ILogger<AdminNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendApprovalRequestAsync(ApprovalRequestDto request)
        {
            _logger.LogInformation(
                "Admin approval request: {EntityType} '{EntityName}' ({EntityId}), Date: {RequestDate}", 
                request.EntityType,
                request.EntityName, 
                request.EntityId,
                request.RequestDate);
                
            
            return Task.CompletedTask;
        }
    }
}