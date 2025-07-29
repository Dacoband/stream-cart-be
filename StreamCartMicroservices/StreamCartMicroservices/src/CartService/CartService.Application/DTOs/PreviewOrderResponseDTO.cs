using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class PreviewOrderResponseDTO
    {
        public int TotalItem {  get; set; }
        public decimal SubTotal {  get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public List<ProductInShopCart> ListCartItem { get; set; }
    }
    
   
}
