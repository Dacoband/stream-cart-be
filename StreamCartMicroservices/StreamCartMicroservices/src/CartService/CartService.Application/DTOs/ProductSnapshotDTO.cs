﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class ProductSnapshotDTO
    {
        public Guid ProductId { get; set; }
        public string? VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public Guid ShopId { get; set; }

        public decimal PriceCurrent { get; set; }
        public decimal PriceOriginal { get; set; }
        public string PrimaryImage { get; set; } = string.Empty;
        public Dictionary<string, string>? Attributes { get; set; } 
        public int Stock { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        
    }
}
