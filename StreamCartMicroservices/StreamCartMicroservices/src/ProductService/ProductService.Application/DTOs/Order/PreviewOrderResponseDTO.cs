using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Order
{
    public class PreviewOrderResponseDTO
    {
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantID { get; set; }
        public string ProductName { get; set; }
        public PriceData PriceData { get; set; }
        public int Quantity { get; set; }
        public string PrimaryImage { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public int StockQuantity { get; set; }
        public bool ProductStatus { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }


    }
    public class PriceData
    {
        public decimal CurrentPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal Discount { get; set; }
    }

}
