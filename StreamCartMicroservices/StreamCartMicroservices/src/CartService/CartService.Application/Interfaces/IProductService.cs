using CartService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductSnapshotDTO> GetProductInfoAsync(string productId, string? variantId);

    }
}
