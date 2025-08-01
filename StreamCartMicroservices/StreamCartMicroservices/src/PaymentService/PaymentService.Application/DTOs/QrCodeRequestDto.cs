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
        /// The unique identifier for the order.
        /// </summary>
        public List<Guid>? OrderIds { get; set; }
    }
}
