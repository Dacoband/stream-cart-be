using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IShopManagementService
    {
        #region CRUD Shop Operations

        /// <summary>
        /// Tạo shop mới
        /// </summary>
        /// <param name="createShopDto">Thông tin shop mới</param>
        /// <param name="createdByAccountId">ID của người tạo</param>
        /// <returns>Shop đã tạo</returns>
        Task<ShopDto> CreateShopAsync(CreateShopDto createShopDto, Guid createdByAccountId);

        /// <summary>
        /// Cập nhật thông tin shop
        /// </summary>
        /// <param name="updateShopCommand">Thông tin cập nhật</param>
        /// <returns>Shop đã cập nhật</returns>
        Task<ShopDto> UpdateShopAsync(UpdateShopCommand updateShopCommand);

        /// <summary>
        /// Xóa shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="deletedByAccountId">ID của người xóa</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<bool> DeleteShopAsync(Guid shopId, Guid deletedByAccountId);

        /// <summary>
        /// Lấy thông tin shop theo ID
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <returns>Shop nếu tìm thấy, null nếu không tìm thấy</returns>
        Task<ShopDto?> GetShopByIdAsync(Guid shopId);

        /// <summary>
        /// Lấy tất cả các shop
        /// </summary>
        /// <returns>Danh sách shop</returns>
        Task<IEnumerable<ShopDto>> GetAllShopsAsync();

        /// <summary>
        /// Lấy danh sách shop phân trang
        /// </summary>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Số lượng mỗi trang</param>
        /// <param name="status">Trạng thái hoạt động (tùy chọn)</param>
        /// <param name="approvalStatus">Trạng thái phê duyệt (tùy chọn)</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tùy chọn)</param>
        /// <param name="sortBy">Sắp xếp theo (tùy chọn)</param>
        /// <param name="ascending">Sắp xếp tăng dần hay không (tùy chọn)</param>
        /// <returns>Kết quả phân trang</returns>
        Task<PagedResult<ShopDto>> GetShopsPagedAsync(
            int pageNumber,
            int pageSize,
            ShopStatus? status = null,
            ApprovalStatus? approvalStatus = null,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true);

        /// <summary>
        /// Lấy danh sách shop theo trạng thái hoạt động
        /// </summary>
        /// <param name="status">Trạng thái hoạt động</param>
        /// <returns>Danh sách shop</returns>
        Task<IEnumerable<ShopDto>> GetShopsByStatusAsync(ShopStatus status);

        /// <summary>
        /// Lấy danh sách shop theo trạng thái phê duyệt
        /// </summary>
        /// <param name="approvalStatus">Trạng thái phê duyệt</param>
        /// <returns>Danh sách shop</returns>
        Task<IEnumerable<ShopDto>> GetShopsByApprovalStatusAsync(ApprovalStatus approvalStatus);

        /// <summary>
        /// Tìm kiếm shop theo tên
        /// </summary>
        /// <param name="nameQuery">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách shop</returns>
        Task<IEnumerable<ShopDto>> SearchShopsByNameAsync(string nameQuery);

        /// <summary>
        /// Lấy danh sách shop theo account ID
        /// </summary>
        /// <param name="accountId">ID của tài khoản</param>
        /// <returns>Danh sách shop</returns>
        Task<IEnumerable<ShopDto>> GetShopsByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Cập nhật thông tin ngân hàng của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="bankingInfo">Thông tin ngân hàng mới</param>
        /// <param name="updatedByAccountId">ID của người cập nhật</param>
        /// <returns>Shop đã cập nhật</returns>
        Task<ShopDto> UpdateShopBankingInfoAsync(Guid shopId, UpdateBankingInfoDto bankingInfo, Guid updatedByAccountId);

        /// <summary>
        /// Kiểm tra tên shop có bị trùng không
        /// </summary>
        /// <param name="shopName">Tên shop</param>
        /// <param name="excludeId">ID shop loại trừ (tùy chọn)</param>
        /// <returns>true nếu tên không trùng, false nếu tên đã tồn tại</returns>
        Task<bool> IsShopNameUniqueAsync(string shopName, Guid? excludeId = null);

        #endregion

        #region Shop Member Management

        /// <summary>
        /// Thêm thành viên vào shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="accountId">ID của tài khoản được thêm</param>
        /// <param name="role">Vai trò trong shop</param>
        /// <param name="addedByAccountId">ID của người thêm</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<bool> AddShopMemberAsync(Guid shopId, Guid accountId, string role, Guid addedByAccountId);

        /// <summary>
        /// Xóa thành viên khỏi shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="accountId">ID của tài khoản bị xóa</param>
        /// <param name="removedByAccountId">ID của người xóa</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<bool> RemoveShopMemberAsync(Guid shopId, Guid accountId, Guid removedByAccountId);

        /// <summary>
        /// Thay đổi vai trò thành viên trong shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="accountId">ID của tài khoản được thay đổi</param>
        /// <param name="newRole">Vai trò mới</param>
        /// <param name="updatedByAccountId">ID của người cập nhật</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<bool> ChangeShopMemberRoleAsync(Guid shopId, Guid accountId, string newRole, Guid updatedByAccountId);

        /// <summary>
        /// Lấy danh sách thành viên của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <returns>Danh sách thành viên</returns>
        Task<IEnumerable<ShopMemberDto>> GetShopMembersAsync(Guid shopId);

        /// <summary>
        /// Kiểm tra xem người dùng có là thành viên của shop không
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="accountId">ID của tài khoản</param>
        /// <returns>true nếu là thành viên, false nếu không phải</returns>
        Task<bool> IsShopMemberAsync(Guid shopId, Guid accountId);

        /// <summary>
        /// Kiểm tra xem người dùng có quyền thực hiện hành động với shop không
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="accountId">ID của tài khoản</param>
        /// <param name="requiredRole">Vai trò yêu cầu tối thiểu</param>
        /// <returns>true nếu có quyền, false nếu không có quyền</returns>
        Task<bool> HasShopPermissionAsync(Guid shopId, Guid accountId, string requiredRole);

        #endregion

        #region Invitation Management

        /// <summary>
        /// Gửi lời mời tham gia shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="email">Email của người được mời</param>
        /// <param name="role">Vai trò được mời</param>
        /// <param name="invitedByAccountId">ID của người mời</param>
        /// <returns>ID của lời mời</returns>
        Task<Guid> SendInvitationAsync(Guid shopId, string email, string role, Guid invitedByAccountId);

        /// <summary>
        /// Chấp nhận lời mời tham gia shop
        /// </summary>
        /// <param name="invitationToken">Token lời mời</param>
        /// <param name="accountId">ID của người chấp nhận</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<bool> AcceptInvitationAsync(string invitationToken, Guid accountId);

        /// <summary>
        /// Từ chối lời mời tham gia shop
        /// </summary>
        /// <param name="invitationToken">Token lời mời</param>
        /// <param name="accountId">ID của người từ chối</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<bool> DeclineInvitationAsync(string invitationToken, Guid accountId);

        #endregion

        #region Shop Status Management

        /// <summary>
        /// Cập nhật trạng thái shop (kích hoạt/vô hiệu hóa)
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="active">true để kích hoạt, false để vô hiệu hóa</param>
        /// <param name="updatedByAccountId">ID của người cập nhật</param>
        /// <returns>Shop đã cập nhật</returns>
        Task<ShopDto> UpdateShopStatusAsync(Guid shopId, bool active, Guid updatedByAccountId);

        /// <summary>
        /// Phê duyệt shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="approvedByAccountId">ID của người phê duyệt</param>
        /// <returns>Shop đã phê duyệt</returns>
        Task<ShopDto> ApproveShopAsync(Guid shopId, Guid approvedByAccountId);

        /// <summary>
        /// Từ chối shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="reason">Lý do từ chối</param>
        /// <param name="rejectedByAccountId">ID của người từ chối</param>
        /// <returns>Shop đã từ chối</returns>
        Task<ShopDto> RejectShopAsync(Guid shopId, string reason, Guid rejectedByAccountId);

        #endregion
    }
}