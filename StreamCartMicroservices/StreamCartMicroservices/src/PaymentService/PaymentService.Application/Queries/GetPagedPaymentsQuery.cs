using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Queries
{
    public class GetPagedPaymentsQuery : IRequest<PagedResult<PaymentDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public PaymentStatus? Status { get; set; }
        public PaymentMethod? Method { get; set; }
        public Guid? UserId { get; set; }
        public Guid? OrderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }
        public bool Ascending { get; set; } = true;
    }
}
