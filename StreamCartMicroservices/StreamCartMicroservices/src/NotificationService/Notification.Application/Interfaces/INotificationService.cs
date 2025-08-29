using Notification.Application.DTOs;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Interfaces
{
    public interface INotificationService
    {
        public Task<ApiResponse<ListNotificationDTO>> GetMyNotification(FilterNotificationDTO filter, string userId);
        public Task<ApiResponse<bool>> MarkAsRead(Guid id);
        public Task<ApiResponse<Notification.Domain.Entities.Notifications>> CreateNotification(CreateNotificationDTO notification);
    }
}
