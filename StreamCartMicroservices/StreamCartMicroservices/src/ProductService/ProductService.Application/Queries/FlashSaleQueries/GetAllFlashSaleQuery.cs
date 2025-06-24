using MediatR;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.FlashSaleQueries
{
    public class GetAllFlashSaleQuery : IRequest<ApiResponse<List<DetailFlashSaleDTO>>>
    {
        public List<Guid>? ProductId { get; set; }
        public List<Guid>? VariantId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public FlashSaleOrderBy? OrderBy { get; set; }
        public OrderDirection? OrderDirection { get; set; }
        public bool? IsActive { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; } = 10;
    }
}
