using System;
using System.Collections.Generic;

namespace ProductService.Application.DTOs.FlashSale
{
    public class FlashSaleSlotInfoDTO
    {
        public DateTime Date { get; set; }
        public int Slot { get; set; }
        public string SlotTimeRange { get; set; } = string.Empty;
        public string SlotStatus { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TotalQuantityAvailable { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DetailFlashSaleDTO> Products { get; set; } = new();
    }

    // ✅ DTO mới chỉ với field yêu cầu
    public class FlashSaleSlotSimpleDTO
    {
        public DateTime Date { get; set; }
        public int Slot { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalProduct { get; set; }
    }

    public class ShopFlashSaleOverviewDTO
    {
        public DateTime Date { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int TotalActiveSlots { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuantityAvailable { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<FlashSaleSlotInfoDTO> Slots { get; set; } = new();
    }

    public class UpdateFlashSaleProductDTO
    {
        public Guid FlashSaleId { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int? QuantityAvailable { get; set; }
    }

    public class BatchUpdateFlashSaleProductsDTO
    {
        public List<UpdateFlashSaleProductDTO> Updates { get; set; } = new();
    }

    
}