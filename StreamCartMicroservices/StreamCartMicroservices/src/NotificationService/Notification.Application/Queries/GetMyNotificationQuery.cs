using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Notification.Application.DTOs;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Queries
{
    public class GetMyNotificationQuery : IRequest<ApiResponse<ListNotificationDTO>>
    {
        public FilterNotificationDTO FilterNotificationDTO { get; set; }
        public string UserId { get; set; }  
    }
}
