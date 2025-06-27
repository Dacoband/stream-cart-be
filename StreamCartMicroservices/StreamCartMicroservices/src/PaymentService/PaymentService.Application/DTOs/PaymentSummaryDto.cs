using System;
using System.Collections.Generic;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;

namespace PaymentService.Application.DTOs
{
    public class PaymentSummaryDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<string, int> PaymentsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PaymentsByMethod { get; set; } = new Dictionary<string, int>();
        public decimal AverageAmount { get; set; }
        public decimal TotalFees { get; set; }
    }
}