using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Domain.Enums
{
    public enum PaymentStatus
    {
        /// <summary>
        /// Payment is pending
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Payment has been successfully processed
        /// </summary>
        Paid = 1,

        /// <summary>
        /// Payment has failed
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Payment has been refunded
        /// </summary>
        Refunded = 3,

        /// <summary>
        /// Payment has been partially refunded
        /// </summary>
        PartiallyRefunded = 4
    }
}
