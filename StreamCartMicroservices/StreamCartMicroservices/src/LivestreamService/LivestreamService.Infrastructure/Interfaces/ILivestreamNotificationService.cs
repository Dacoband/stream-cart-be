using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Interfaces
{
    public interface ILivestreamNotificationService
    {
        Task SendLivestreamTimeWarningAsync(Guid livestreamId, Guid sellerId, int remainingMinutes);
        Task SendLivestreamTimeExpiredAsync(Guid livestreamId, Guid sellerId);
    }
}
