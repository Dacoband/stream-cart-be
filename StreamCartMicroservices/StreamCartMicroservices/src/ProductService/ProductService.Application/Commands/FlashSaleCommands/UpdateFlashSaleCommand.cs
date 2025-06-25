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
    public class UpdateFlashSaleCommand : IRequest<ApiResponse<DetailFlashSaleDTO>>
    {
        public string UserId { get; set; }
        public string ShopId { get; set; }
        public string FlashSaleId { get; set; }
        public decimal? FLashSalePrice { get; set; }
        public int? QuantityAvailable { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
