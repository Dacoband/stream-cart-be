using MediatR;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Application.Queries;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Handlers
{
    public class GetMyNotificationHandler : IRequestHandler<GetMyNotificationQuery, ApiResponse<ListNotificationDTO>>
    {
        private readonly INotificationService _notificationService;
        public GetMyNotificationHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        public Task<ApiResponse<ListNotificationDTO>> Handle(GetMyNotificationQuery request, CancellationToken cancellationToken)
        {
            return _notificationService.GetMyNotification(request.FilterNotificationDTO, request.UserId);
        }
    }
}
