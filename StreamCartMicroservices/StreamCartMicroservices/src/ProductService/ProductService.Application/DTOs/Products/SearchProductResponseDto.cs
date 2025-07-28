using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Products
{
    public class SearchProductResponseDto
    {
        public PagedResult<ProductSearchItemDto> Products { get; set; } = new PagedResult<ProductSearchItemDto>(new List<ProductSearchItemDto>(), 0, 1, 20);
        public int TotalResults { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public double SearchTimeMs { get; set; }
        public List<string> SuggestedKeywords { get; set; } = new List<string>();
        public SearchFiltersDto AppliedFilters { get; set; } = new SearchFiltersDto();
    }
}
