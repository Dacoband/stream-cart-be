using ProductService.Application.DTOs.FlashSale;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IFlashSaleService
    {
        public Task<ApiResponse<List<DetailFlashSaleDTO>>> CreateFlashSale(CreateFlashSaleDTO request, string userId, string shopId);
        public Task<ApiResponse<DetailFlashSaleDTO>> UpdateFlashSale(UpdateFlashSaleDTO request, string flashSaleId, string userId, string shopId);
        public Task<ApiResponse<List<DetailFlashSaleDTO>>> FilterFlashSale(FilterFlashSaleDTO filterFlashSale);
        public Task<ApiResponse<DetailFlashSaleDTO>> GetFlashSaleById(string id);
        public Task<ApiResponse<bool>> DeleteFlashsale(string id, string userId, string shopId);
        Task<ApiResponse<List<DetailFlashSaleDTO>>> GetFlashSalesByShopIdAsync(string shopId, FilterFlashSaleDTO filter);

    }
}
