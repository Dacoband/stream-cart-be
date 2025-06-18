using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Domain.Enums
{
    /// <summary>
    /// Trạng thái phê duyệt của shop
    /// </summary>
    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }
    /// <summary>
    /// Trạng thái hoạt động của shop
    /// </summary>
    public enum ShopStatus
    {
        Inactive,
        Active
    }
}
