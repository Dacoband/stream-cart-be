using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Command
{
    public class MarkAsRead : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
    }
}
