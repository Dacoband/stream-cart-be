using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs.Products
{
    public class SearchProductRequestDto
    {
        /// <summary>
        /// Từ khóa tìm kiếm sản phẩm
        /// </summary>
        [Required(ErrorMessage = "Từ khóa tìm kiếm không được để trống")]
        [StringLength(255, ErrorMessage = "Từ khóa tìm kiếm không được quá 255 ký tự")]
        public string SearchTerm { get; set; } = string.Empty;

        /// <summary>
        /// Số trang (mặc định = 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Số sản phẩm mỗi trang (mặc định = 20, max = 100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Số sản phẩm mỗi trang phải từ 1 đến 100")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Lọc theo danh mục (tùy chọn)
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Giá tối thiểu
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Giá tối đa
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa phải lớn hơn hoặc bằng 0")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Lọc theo shop (tùy chọn)
        /// </summary>
        public Guid? ShopId { get; set; }

        /// <summary>
        /// Sắp xếp theo: "price_asc", "price_desc", "newest", "best_selling", "rating"
        /// </summary>
        public string SortBy { get; set; } = "relevance";

        /// <summary>
        /// Chỉ hiển thị sản phẩm có trong kho
        /// </summary>
        public bool InStockOnly { get; set; } = false;

        /// <summary>
        /// Đánh giá tối thiểu (1-5 sao)
        /// </summary>
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int? MinRating { get; set; }

        /// <summary>
        /// Chỉ hiển thị sản phẩm đang giảm giá
        /// </summary>
        public bool OnSaleOnly { get; set; } = false;
    }
}