using ProductService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.DTOs
{
    public class QrCodeRequestDto
    {
        /// <summary>
        /// The amount to be paid, in the smallest currency unit (e.g., cents for USD).
        /// </summary>
        public long Amount { get; set; }
        /// <summary>
        /// The currency code (e.g., "USD", "VND").
        /// </summary>
        public PaymentMethod paymentMethod { get; set; }
        /// <summary>
        /// The description of the payment.
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// The unique identifier for the order.
        /// </summary>
        public Guid OrderId { get; set; } 
    }
}
