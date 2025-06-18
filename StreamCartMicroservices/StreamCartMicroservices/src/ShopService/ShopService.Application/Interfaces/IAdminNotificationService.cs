using ShopService.Application.DTOs;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IAdminNotificationService
    {
        Task SendApprovalRequestAsync(ApprovalRequestDto request);
    }
}