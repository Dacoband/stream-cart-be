using LivestreamService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface ILivestreamProductRepository : IGenericRepository<LivestreamProduct>
    {
        Task<IEnumerable<LivestreamProduct>> GetByLivestreamIdAsync(Guid livestreamId);
        Task<LivestreamProduct> GetLivestreamProductAsync(Guid id);
        Task<bool> ExistsByProductInLivestreamAsync(Guid livestreamId, string productId, string variantId);
        Task<IEnumerable<LivestreamProduct>> GetPinnedProductsAsync(Guid livestreamId, int limit = 5);

        // New methods
        Task<IEnumerable<LivestreamProduct>> GetFlashSaleProductsAsync(Guid livestreamId);

        Task<bool> UpdateDisplayOrderAsync(Guid id, int displayOrder);
        Task<IEnumerable<LivestreamProduct>> GetProductsOrderedByDisplayAsync(Guid livestreamId);
        Task<LivestreamProduct?> GetByCompositeKeyAsync(Guid livestreamId, string productId, string variantId);
        Task<IEnumerable<LivestreamProduct>> GetAllPinnedProductsByLivestreamAsync(Guid livestreamId);
        Task UnpinAllProductsInLivestreamAsync(Guid livestreamId, string modifiedBy);
        Task<LivestreamProduct?> GetCurrentPinnedProductAsync(Guid livestreamId);
    }
}
