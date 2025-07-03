using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Events;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using Shared.Common.Domain.Bases;

namespace ShopService.Application.Services
{
    public class ShopManagementService : IShopManagementService
    {
        private readonly IShopRepository _shopRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IMediator _mediator;
        private readonly ILogger<ShopManagementService> _logger;
        private readonly IMessagePublisher _messagePublisher;

        public ShopManagementService(
            IShopRepository shopRepository,
            IAccountServiceClient accountServiceClient,
            IMediator mediator,
            ILogger<ShopManagementService> logger,
            IMessagePublisher messagePublisher)
        {
            _shopRepository = shopRepository;
            _accountServiceClient = accountServiceClient;
            _mediator = mediator;
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        #region CRUD Shop Operations

        public async Task<ShopDto> CreateShopAsync(CreateShopDto createShopDto, Guid createdByAccountId)
        {
            try
            {
                var command = new CreateShopCommand
                {
                    ShopName = createShopDto.ShopName,
                    Description = createShopDto.Description,
                    LogoURL = createShopDto.LogoURL,
                    CoverImageURL = createShopDto.CoverImageURL,
                    AccountId = createdByAccountId,
                    CreatedBy = createdByAccountId.ToString()
                };

                var result = await _mediator.Send(command);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo shop mới bởi tài khoản {AccountId}", createdByAccountId);
                throw;
            }
        }

        public async Task<ShopDto> UpdateShopAsync(UpdateShopCommand updateShopCommand)
        {
            try
            {
                var result = await _mediator.Send(updateShopCommand);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật shop {ShopId}", updateShopCommand.Id);
                throw;
            }
        }

        public async Task<bool> DeleteShopAsync(Guid shopId, Guid deletedByAccountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể xóa: Shop {ShopId} không tồn tại", shopId);
                    return false;
                }

                // Kiểm tra người thực hiện có quyền xóa shop
                if (!await HasShopPermissionAsync(shopId, deletedByAccountId, "Owner"))
                {
                    _logger.LogWarning("Tài khoản {AccountId} không có quyền xóa shop {ShopId}", deletedByAccountId, shopId);
                    return false;
                }

                // Đánh dấu xóa (soft delete)
                shop.Delete(deletedByAccountId.ToString());
                await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

                // Publish event
                await _messagePublisher.PublishAsync(new ShopStatusChanged
                {
                    ShopId = shopId,
                    ShopName = shop.ShopName,
                    Status = "Deleted",
                    AccountId = deletedByAccountId,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("Đã xóa shop {ShopId} bởi {AccountId}", shopId, deletedByAccountId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa shop {ShopId}", shopId);
                return false;
            }
        }

        public async Task<ShopDto?> GetShopByIdAsync(Guid shopId)
        {
            try
            {
                var query = new GetShopByIdQuery { Id = shopId };
                var result = await _mediator.Send(query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin shop {ShopId}", shopId);
                return null;
            }
        }

        public async Task<IEnumerable<ShopDto>> GetAllShopsAsync()
        {
            try
            {
                var shops = await _shopRepository.GetAllAsync();
                return shops.Select(MapShopToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả các shop");
                return Enumerable.Empty<ShopDto>();
            }
        }

        public async Task<PagedResult<ShopDto>> GetShopsPagedAsync(
            int pageNumber, 
            int pageSize,
            ShopStatus? status = null,
            ApprovalStatus? approvalStatus = null,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true)
        {
            try
            {
                var pagedShops = await _shopRepository.GetPagedShopsAsync(
                    pageNumber,
                    pageSize,
                    status,
                    approvalStatus,
                    searchTerm,
                    sortBy,
                    ascending);

                var shopDtos = pagedShops.Items.Select(MapShopToDto).ToList();

                return new PagedResult<ShopDto>
                {
                    Items = shopDtos,
                    CurrentPage = pagedShops.CurrentPage,
                    PageSize = pagedShops.PageSize,
                    TotalCount = pagedShops.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách shop phân trang");
                return new PagedResult<ShopDto>
                {
                    Items = Enumerable.Empty<ShopDto>(),
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        public async Task<IEnumerable<ShopDto>> GetShopsByStatusAsync(ShopStatus status)
        {
            try
            {
                var shops = await _shopRepository.GetByStatusAsync(status);
                return shops.Select(MapShopToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách shop theo trạng thái {Status}", status);
                return Enumerable.Empty<ShopDto>();
            }
        }

        public async Task<IEnumerable<ShopDto>> GetShopsByApprovalStatusAsync(ApprovalStatus approvalStatus)
        {
            try
            {
                var query = new GetShopsByApprovalStatusQuery { Status = approvalStatus };
                var result = await _mediator.Send(query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách shop theo trạng thái phê duyệt {ApprovalStatus}", approvalStatus);
                return Enumerable.Empty<ShopDto>();
            }
        }

        public async Task<IEnumerable<ShopDto>> SearchShopsByNameAsync(string nameQuery)
        {
            try
            {
                var query = new SearchShopsQuery { SearchTerm = nameQuery };
                var result = await _mediator.Send(query);
                return result.Items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm shop với từ khóa {NameQuery}", nameQuery);
                return Enumerable.Empty<ShopDto>();
            }
        }

        public async Task<IEnumerable<ShopDto>> GetShopsByAccountIdAsync(Guid accountId)
        {
            try
            {
                var query = new GetShopsByOwnerIdQuery { AccountId = accountId };
                var result = await _mediator.Send(query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách shop của tài khoản {AccountId}", accountId);
                return Enumerable.Empty<ShopDto>();
            }
        }

        public async Task<ShopDto> UpdateShopBankingInfoAsync(Guid shopId, UpdateBankingInfoDto bankingInfo, Guid updatedByAccountId)
        {
            try
            {
                var command = new UpdateBankingInfoCommand
                {
                    ShopId = shopId,
                    BankAccountNumber = bankingInfo.BankAccountNumber,
                    BankName = bankingInfo.BankName,
                    TaxNumber = bankingInfo.TaxNumber,
                    UpdatedBy = updatedByAccountId.ToString()
                };

                var result = await _mediator.Send(command);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin ngân hàng của shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<bool> IsShopNameUniqueAsync(string shopName, Guid? excludeId = null)
        {
            try
            {
                return await _shopRepository.IsNameUniqueAsync(shopName, excludeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra tên shop {ShopName}", shopName);
                return false;
            }
        }

        #endregion

        #region Shop Member Management

        public async Task<bool> AddShopMemberAsync(Guid shopId, Guid accountId, string role, Guid addedByAccountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể thêm thành viên: Shop {ShopId} không tồn tại", shopId);
                    return false;
                }

                // Kiểm tra người thực hiện có quyền thêm thành viên
                if (!await HasShopPermissionAsync(shopId, addedByAccountId, "Owner"))
                {
                    _logger.LogWarning("Tài khoản {AccountId} không có quyền thêm thành viên vào shop {ShopId}", addedByAccountId, shopId);
                    return false;
                }

                // Cập nhật thông tin liên kết Account-Shop
                await _accountServiceClient.UpdateAccountShopInfoAsync(accountId, shopId);

                _logger.LogInformation("Đã thêm thành viên {AccountId} vào shop {ShopId} với vai trò {Role}", accountId, shopId, role);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm thành viên {AccountId} vào shop {ShopId}", accountId, shopId);
                return false;
            }
        }

        public async Task<bool> RemoveShopMemberAsync(Guid shopId, Guid accountId, Guid removedByAccountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể xóa thành viên: Shop {ShopId} không tồn tại", shopId);
                    return false;
                }

                // Kiểm tra người thực hiện có quyền xóa thành viên
                if (!await HasShopPermissionAsync(shopId, removedByAccountId, "Owner"))
                {
                    _logger.LogWarning("Tài khoản {AccountId} không có quyền xóa thành viên khỏi shop {ShopId}", removedByAccountId, shopId);
                    return false;
                }

                // Không cho phép xóa Owner
                if (await HasShopPermissionAsync(shopId, accountId, "Owner"))
                {
                    _logger.LogWarning("Không thể xóa chủ sở hữu khỏi shop {ShopId}", shopId);
                    return false;
                }

                // Cập nhật thông tin liên kết Account-Shop (gán null cho ShopId)
                await _accountServiceClient.UpdateAccountShopInfoAsync(accountId, Guid.Empty);

                _logger.LogInformation("Đã xóa thành viên {AccountId} khỏi shop {ShopId}", accountId, shopId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa thành viên {AccountId} khỏi shop {ShopId}", accountId, shopId);
                return false;
            }
        }

        public async Task<bool> ChangeShopMemberRoleAsync(Guid shopId, Guid accountId, string newRole, Guid updatedByAccountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể thay đổi vai trò: Shop {ShopId} không tồn tại", shopId);
                    return false;
                }

                // Kiểm tra người thực hiện có quyền thay đổi vai trò
                if (!await HasShopPermissionAsync(shopId, updatedByAccountId, "Owner"))
                {
                    _logger.LogWarning("Tài khoản {AccountId} không có quyền thay đổi vai trò trong shop {ShopId}", updatedByAccountId, shopId);
                    return false;
                }

                // Không cho phép thay đổi vai trò Owner
                if (await HasShopPermissionAsync(shopId, accountId, "Owner") && newRole != "Owner")
                {
                    _logger.LogWarning("Không thể thay đổi vai trò của chủ sở hữu shop {ShopId}", shopId);
                    return false;
                }

                // TODO: Cập nhật vai trò trong bảng liên kết Account-Shop

                _logger.LogInformation("Đã thay đổi vai trò của thành viên {AccountId} trong shop {ShopId} thành {Role}", accountId, shopId, newRole);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi vai trò của thành viên {AccountId} trong shop {ShopId}", accountId, shopId);
                return false;
            }
        }

        public async Task<IEnumerable<ShopMemberDto>> GetShopMembersAsync(Guid shopId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể lấy danh sách thành viên: Shop {ShopId} không tồn tại", shopId);
                    return Enumerable.Empty<ShopMemberDto>();
                }

                // TODO: Lấy danh sách thành viên từ bảng liên kết Account-Shop
                // Hiện tại trả về danh sách rỗng cho đến khi triển khai
                return Enumerable.Empty<ShopMemberDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thành viên của shop {ShopId}", shopId);
                return Enumerable.Empty<ShopMemberDto>();
            }
        }

        public async Task<bool> IsShopMemberAsync(Guid shopId, Guid accountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    return false;
                }

                // Kiểm tra account có phải là thành viên của shop không
                var shopForAccount = await _shopRepository.GetByIdForAccountAsync(shopId, accountId);
                return shopForAccount != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra thành viên {AccountId} trong shop {ShopId}", accountId, shopId);
                return false;
            }
        }

        public async Task<bool> HasShopPermissionAsync(Guid shopId, Guid accountId, string requiredRole)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    return false;
                }

                // Kiểm tra account có phải là thành viên của shop không
                var shopForAccount = await _shopRepository.GetByIdForAccountAsync(shopId, accountId);
                if (shopForAccount == null)
                {
                    return false;
                }

                // TODO: Kiểm tra vai trò của account trong shop
                // Hiện tại giả định account chỉ có quyền nếu là Owner
                // Cần triển khai kiểm tra vai trò chi tiết sau
                return true; // Tạm thời luôn trả về true để test
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền của thành viên {AccountId} trong shop {ShopId}", accountId, shopId);
                return false;
            }
        }

        #endregion

        #region Invitation Management

        public async Task<Guid> SendInvitationAsync(Guid shopId, string email, string role, Guid invitedByAccountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể gửi lời mời: Shop {ShopId} không tồn tại", shopId);
                    return Guid.Empty;
                }

                // Kiểm tra người mời có quyền mời thành viên
                if (!await HasShopPermissionAsync(shopId, invitedByAccountId, "Owner"))
                {
                    _logger.LogWarning("Tài khoản {AccountId} không có quyền mời thành viên vào shop {ShopId}", invitedByAccountId, shopId);
                    return Guid.Empty;
                }

                // TODO: Tạo lời mời và lưu vào database
                
                // TODO: Gửi email thông báo

                _logger.LogInformation("Đã gửi lời mời đến {Email} tham gia shop {ShopId} với vai trò {Role}", email, shopId, role);
                return Guid.NewGuid(); // Tạm thời trả về một ID ngẫu nhiên
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi lời mời đến {Email} tham gia shop {ShopId}", email, shopId);
                return Guid.Empty;
            }
        }

        public async Task<bool> AcceptInvitationAsync(string invitationToken, Guid accountId)
        {
            try
            {
                // TODO: Tìm lời mời dựa trên token và kiểm tra tính hợp lệ
                
                // TODO: Thêm thành viên vào shop

                _logger.LogInformation("Tài khoản {AccountId} đã chấp nhận lời mời tham gia shop", accountId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấp nhận lời mời tham gia shop của tài khoản {AccountId}", accountId);
                return false;
            }
        }

        public async Task<bool> DeclineInvitationAsync(string invitationToken, Guid accountId)
        {
            try
            {
                // TODO: Tìm lời mời dựa trên token và kiểm tra tính hợp lệ
                
                // TODO: Từ chối lời mời

                _logger.LogInformation("Tài khoản {AccountId} đã từ chối lời mời tham gia shop", accountId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi từ chối lời mời tham gia shop của tài khoản {AccountId}", accountId);
                return false;
            }
        }

        #endregion

        #region Shop Status Management

        public async Task<ShopDto> UpdateShopStatusAsync(Guid shopId, bool active, Guid updatedByAccountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể cập nhật trạng thái: Shop {ShopId} không tồn tại", shopId);
                    return null;
                }

                // Kiểm tra người thực hiện có quyền cập nhật trạng thái
                if (!await HasShopPermissionAsync(shopId, updatedByAccountId, "Owner"))
                {
                    _logger.LogWarning("Tài khoản {AccountId} không có quyền cập nhật trạng thái của shop {ShopId}", updatedByAccountId, shopId);
                    return null;
                }

                // Cập nhật trạng thái
                if (active)
                {
                    shop.Activate(updatedByAccountId.ToString());
                }
                else
                {
                    shop.Deactivate(updatedByAccountId.ToString());
                }

                // Lưu thay đổi
                await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

                // Chuyển đổi sang DTO
                var shopDto = MapShopToDto(shop);

                _logger.LogInformation("Đã cập nhật trạng thái của shop {ShopId} thành {Status}", shopId, active ? "Active" : "Inactive");
                return shopDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái của shop {ShopId}", shopId);
                return null;
            }
        }

        public async Task<ShopDto> ApproveShopAsync(Guid shopId, Guid approvedByAccountId)
        {
            try
            {
                var command = new ApproveShopCommand
                {
                    ShopId = shopId,
                    ApprovedBy = approvedByAccountId.ToString()
                };

                var result = await _mediator.Send(command);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phê duyệt shop {ShopId}", shopId);
                return null;
            }
        }

        public async Task<ShopDto> RejectShopAsync(Guid shopId, string reason, Guid rejectedByAccountId)
        {
            try
            {
                var command = new RejectShopCommand
                {
                    ShopId = shopId,
                    RejectionReason = reason,
                    RejectedBy = rejectedByAccountId.ToString()
                };

                var result = await _mediator.Send(command);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi từ chối shop {ShopId}", shopId);
                return null;
            }
        }
        public async Task<ShopDto> UpdateProductCountAsync(Guid shopId, int totalProduct, Guid updatedByAccountId)
        {
            try
            {
                // Kiểm tra shop tồn tại
                var shop = await _shopRepository.GetByIdAsync(shopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể cập nhật số lượng sản phẩm: Shop {ShopId} không tồn tại", shopId);
                    return null;
                }

                // Kiểm tra người thực hiện có quyền cập nhật
                if (!await HasShopPermissionAsync(shopId, updatedByAccountId, "Owner"))
                {
                    _logger.LogWarning("Tài khoản {AccountId} không có quyền cập nhật số lượng sản phẩm của shop {ShopId}", updatedByAccountId, shopId);
                    return null;
                }

                // Cập nhật số lượng sản phẩm
                shop.UpdateProductCount(totalProduct, updatedByAccountId.ToString());

                // Lưu thay đổi
                await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

                // Chuyển đổi sang DTO
                var shopDto = MapShopToDto(shop);

                _logger.LogInformation("Đã cập nhật số lượng sản phẩm của shop {ShopId} thành {TotalProduct}", shopId, totalProduct);
                return shopDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng sản phẩm của shop {ShopId}", shopId);
                return null;
            }
        }

        #endregion

        #region Helper Methods

        private ShopDto MapShopToDto(Shop shop)
        {
            return new ShopDto
            {
                Id = shop.Id,
                ShopName = shop.ShopName,
                Description = shop.Description,
                LogoURL = shop.LogoURL,
                CoverImageURL = shop.CoverImageURL,
                RatingAverage = shop.RatingAverage,
                TotalReview = shop.TotalReview,
                RegistrationDate = shop.RegistrationDate,
                ApprovalStatus = shop.ApprovalStatus.ToString(),
                ApprovalDate = shop.ApprovalDate,
                BankAccountNumber = shop.BankAccountNumber,
                BankName = shop.BankName,
                TaxNumber = shop.TaxNumber,
                TotalProduct = shop.TotalProduct,
                CompleteRate = shop.CompleteRate,
                Status = shop.Status == ShopStatus.Active,
                CreatedAt = shop.CreatedAt,
                CreatedBy = shop.CreatedBy,
                LastModifiedAt = shop.LastModifiedAt,
                LastModifiedBy = shop.LastModifiedBy,
                AccountId = Guid.Empty
            };
        }

        #endregion
    }
}
