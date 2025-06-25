using MediatR;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.FlashSaleCommands
{
    public class CreateFlashSaleCommand : IRequest<ApiResponse<List<DetailFlashSaleDTO>>>
    {
        public string UserId { get; set; }
        public string ShopId { get; set; }
        public Guid ProductId { get; set; }
        public List<Guid>? VariantId { get; set; }
        [Range(100, double.MaxValue, ErrorMessage = "Giá FalshSale phải lớn hơn 100đ")]
        public decimal FLashSalePrice { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm áp dụng FlashSale phải lớn hơn 0")]
        public int? QuantityAvailable { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
