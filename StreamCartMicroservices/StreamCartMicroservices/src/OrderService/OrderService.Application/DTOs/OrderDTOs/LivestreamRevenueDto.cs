using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.OrderDTOs
{
    /// <summary>
    /// DTO cho doanh thu và sản phẩm của livestream
    /// </summary>
    public class LivestreamRevenueDto
    {
        /// <summary>
        /// ID của livestream
        /// </summary>
        public Guid LivestreamId { get; set; }

        /// <summary>
        /// Tổng doanh thu của livestream
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Tổng số đơn hàng của livestream
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Danh sách sản phẩm có đơn hàng trong livestream
        /// </summary>
        public List<LivestreamProductSalesDto> ProductsWithOrders { get; set; } = new();
    }

    /// <summary>
    /// DTO cho sản phẩm có đơn hàng trong livestream (bao gồm cả variant)
    /// </summary>
    public class LivestreamProductSalesDto
    {
        /// <summary>
        /// ID sản phẩm
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// ID variant (nếu có)
        /// </summary>
        public Guid? VariantId { get; set; }

        /// <summary>
        /// Tên sản phẩm
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Tên variant (nếu có)
        /// </summary>
        public string? VariantName { get; set; }

        /// <summary>
        /// SKU của variant (nếu có)
        /// </summary>
        public string? VariantSKU { get; set; }

        /// <summary>
        /// URL hình ảnh sản phẩm
        /// </summary>
        public string ProductImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng đã bán
        /// </summary>
        public int QuantitySold { get; set; } // ✅ BẮT BUỘC PHẢI CÓ!

        /// <summary>
        /// Doanh thu từ sản phẩm/variant này
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Giá bán của sản phẩm/variant
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Tên hiển thị đầy đủ (sản phẩm + variant)
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(VariantName)
            ? ProductName
            : $"{ProductName} - {VariantName}";
    }
}