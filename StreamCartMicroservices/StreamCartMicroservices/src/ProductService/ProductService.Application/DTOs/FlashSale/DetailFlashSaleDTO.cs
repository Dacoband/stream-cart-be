﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.FlashSale
{
    public class DetailFlashSaleDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public decimal FlashSalePrice { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantitySold { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}
