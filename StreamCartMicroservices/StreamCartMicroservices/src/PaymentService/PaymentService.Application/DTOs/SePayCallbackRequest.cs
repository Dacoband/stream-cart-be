using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.DTOs
{
    public class SePayCallbackRequest
    {
        public string? TransactionId { get; set; }
        public string? OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
       
    }
}
