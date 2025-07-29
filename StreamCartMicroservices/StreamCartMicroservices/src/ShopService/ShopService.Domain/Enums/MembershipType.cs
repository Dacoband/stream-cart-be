using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Domain.Enums
{
    public enum MembershipType
    {
        /// <summary>
        /// Gói gia hạn
        /// </summary>
        Renewal = 0,

        /// <summary>
        /// Gói mới
        /// </summary>
        New = 1
    }

}
