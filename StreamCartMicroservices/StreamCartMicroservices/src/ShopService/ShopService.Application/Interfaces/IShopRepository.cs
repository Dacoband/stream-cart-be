using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Common.Domain.Bases;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using Shared.Common.Data.Interfaces;
    
namespace ShopService.Application.Interfaces
{
    public interface IShopRepository : IGenericRepository<Shop>
    {
        /// <summary>
        /// Lấy danh sách shop theo trạng thái phê duyệt
        /// </summary>
        Task<IEnumerable<Shop>> GetByApprovalStatusAsync(ApprovalStatus status);
        
        /// <summary>
        /// Lấy danh sách shop theo trạng thái hoạt động
        /// </summary>
        Task<IEnumerable<Shop>> GetByStatusAsync(ShopStatus status);
        
        /// <summary>
        /// Tìm shop theo tên (tìm kiếm mờ)
        /// </summary>
        Task<IEnumerable<Shop>> SearchByNameAsync(string nameQuery);
        
        /// <summary>
        /// Lấy danh sách shop xếp hạng cao nhất
        /// </summary>
        Task<IEnumerable<Shop>> GetTopRatedShopsAsync(int count);
        
        /// <summary>
        /// Kiểm tra xem tên shop có bị trùng không
        /// </summary>
        Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);

        /// <summary>
        /// Lấy shop theo ID và kiểm tra xem account có quyền truy cập không
        /// </summary>
        Task<Shop?> GetByIdForAccountAsync(Guid shopId, Guid accountId);

        /// <summary>
        /// Lấy danh sách shop mà account là thành viên
        /// </summary>
        Task<IEnumerable<Shop>> GetShopsByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Lấy danh sách shop phân trang
        /// </summary>
        Task<PagedResult<Shop>> GetPagedShopsAsync(
            int pageNumber, 
            int pageSize, 
            ShopStatus? status = null, 
            ApprovalStatus? approvalStatus = null, 
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true);
    }
}
