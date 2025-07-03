using MediatR;
using Notification.Application.Command;
using Notification.Application.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Handlers
{
    public class MarkAsReadHandler : IRequestHandler<MarkAsRead, ApiResponse<bool>>
    {
        private readonly INotificationService _notificationService;
        public MarkAsReadHandler(INotificationService notificationService)
        {
             _notificationService = notificationService;
        }
        public async Task<ApiResponse<bool>> Handle(MarkAsRead request, CancellationToken cancellationToken)
        {
            return await _notificationService.MarkAsRead(request.Id);
        }
    }
}
