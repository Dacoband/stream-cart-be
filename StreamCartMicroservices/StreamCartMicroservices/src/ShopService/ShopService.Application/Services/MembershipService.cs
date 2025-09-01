using Microsoft.EntityFrameworkCore;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly IMembershipRepository _membershipRepository;
        public MembershipService(IMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }
        public async Task<ApiResponse<DetailMembershipDTO>> CreateMembership(CreateMembershipDTO request, string userId)
        {
            var result = new ApiResponse<DetailMembershipDTO>()
            {
                Success = true,
                Message = "Tạo gói thành viên thành công"
            };
            Membership membership = new Membership()
            {
                Name = request.Name,
                Description = request.Description,
                Commission = request.Commission,
                MaxLivestream = request.MaxLivestream,
                MaxProduct = request.MaxModerator,
                Duration = request.Duration,
                Price = request.Price,
                Type =request.Type.ToString(),
                
            };
            membership.SetCreator(userId);
            try
            {
                await _membershipRepository.InsertAsync(membership);
                result.Data = new DetailMembershipDTO() { 
                    MembershipId = membership.Id.ToString(),
                    Price = membership.Price,
                    Description = membership.Description,
                    Commission= membership.Commission,
                    CreatedAt = membership.CreatedAt,
                    CreatedBy = membership.CreatedBy,
                    Duration = membership.Duration,
                    MaxLivestream= membership.MaxLivestream,
                    MaxProduct= membership.MaxProduct,
                    Name = membership.Name,
                    Type = membership.Type.ToString(),
                    UpdatedAt   = membership.LastModifiedAt,
                    UpdatedBy = membership.LastModifiedBy,
                };
                return result;
            }
            catch (Exception ex) {
            
            result.Success = false;
            result.Message = ex.Message;
            return result;

            }

        }

        public async Task<ApiResponse<DetailMembershipDTO>> DeleteMembership(string request, string userId)
        {
            var result = new ApiResponse<DetailMembershipDTO>()
            {
                Success = true,
                Message = "Xóa gói thành viên thành công"
            };
            var existingMembership = await _membershipRepository.GetByIdAsync(request);
            if (existingMembership == null) {
                result.Success=false;
                result.Message = "Không tìm thấy gói thành viên";
                return result;
            }
            try
            {
                if (existingMembership.IsDeleted == false) { existingMembership.Delete(userId); }
                else
                {
                    existingMembership.Restore(userId);
                }

                await _membershipRepository.ReplaceAsync(existingMembership.Id.ToString(),existingMembership);
                result.Data = new DetailMembershipDTO()
                {
                    MembershipId = existingMembership.Id.ToString(),
                    Price = existingMembership.Price,
                    Description = existingMembership.Description,
                    Commission = existingMembership.Commission,
                    CreatedAt = existingMembership.CreatedAt,
                    CreatedBy = existingMembership.CreatedBy,
                    Duration = existingMembership.Duration,
                    MaxLivestream = existingMembership.MaxLivestream,
                    MaxProduct = existingMembership.MaxProduct,
                    Name = existingMembership.Name,
                    Type = existingMembership.Type.ToString(),
                    UpdatedAt = existingMembership.LastModifiedAt,
                    UpdatedBy = existingMembership.LastModifiedBy,
                    IsDeleted = existingMembership.IsDeleted,
                };
                return result;
            }
            catch (Exception ex)
            {

                result.Success = false;
                result.Message = ex.Message;
                return result;

            }
        }

        public async Task<ApiResponse<ListMembershipDTO>> FilterMembership(FilterMembershipDTO request)
        {
            var result = new ApiResponse<ListMembershipDTO>()
            {
                Success = true,
                Message = "Lọc gói thành viên thành công"
            };
            var query =await _membershipRepository.GetAll();

            // Lọc theo điều kiện
            if (!string.IsNullOrEmpty(request.Type))
            {
                query = query.Where(m => m.Type == request.Type);
            }

            if (request.FromPrice.HasValue)
            {
                query = query.Where(m => m.Price >= request.FromPrice.Value);
            }

            if (request.ToPrice.HasValue)
            {
                query = query.Where(m => m.Price <= request.ToPrice.Value);
            }

            if (request.MinDuration.HasValue)
            {
                query = query.Where(m => m.Duration >= request.MinDuration.Value);
            }

            if (request.MaxProduct.HasValue)
            {
                query = query.Where(m => m.MaxProduct <= request.MaxProduct.Value);
            }

            if (request.MaxLivestream.HasValue)
            {
                query = query.Where(m => m.MaxLivestream <= request.MaxLivestream.Value);
            }

            if (request.MaxCommission.HasValue)
            {
                query = query.Where(m => m.Commission <= request.MaxCommission.Value);
            }

            //if (request.IsDeleted.HasValue)
            //{
            //    query = query.Where(m => m.IsDeleted == request.IsDeleted.Value);
            //}

            // Sắp xếp
            query = (request.SortBy, request.SortDirection) switch
            {
                (SortByMembershipEnum.Price, SortDirectionEnum.Asc) => query.OrderBy(m => m.Price),
                (SortByMembershipEnum.Price, SortDirectionEnum.Desc) => query.OrderByDescending(m => m.Price),
                (SortByMembershipEnum.Name, SortDirectionEnum.Desc) => query.OrderByDescending(m => m.Name),
                _ => query.OrderBy(m => m.Name) // Default: SortBy Name ASC
            };

            // Phân trang
            var totalItems = query.Count();
            var pageIndex = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? 10;

            var items = query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new DetailMembershipDTO
                {
                    MembershipId = m.Id.ToString(),
                    Name = m.Name,
                    Type = m.Type,
                    Description = m.Description,
                    Price = m.Price,
                    Duration = m.Duration,
                    MaxProduct = m.MaxProduct,
                    MaxLivestream = m.MaxLivestream,
                    Commission = m.Commission,
                    CreatedBy = m.CreatedBy,
                    UpdatedBy = m.LastModifiedBy,
                    UpdatedAt = m.LastModifiedAt,
                    IsDeleted = m.IsDeleted
                })
                .ToList();


            result.Data = new ListMembershipDTO
            {
                Memberships = items,
                TotalItems = totalItems,
            };

            return result;
        }

        public async Task<ApiResponse<DetailMembershipDTO>> GetMembershipById(string id)
        {
            var result = new ApiResponse<DetailMembershipDTO>()
            {
                Success = true,
                Message = "Lấy dữ liệu gói thành viên thành công"
            };
            var existingMembership = await _membershipRepository.GetById(id);
            if (existingMembership == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy gói thành viên";
                return result;
            }
            result.Data = new DetailMembershipDTO()
            {
                MembershipId = existingMembership.Id.ToString(),
                Price = existingMembership.Price,
                Description = existingMembership.Description,
                Commission = existingMembership.Commission,
                CreatedAt = existingMembership.CreatedAt,
                CreatedBy = existingMembership.CreatedBy,
                Duration = existingMembership.Duration,
                MaxLivestream = existingMembership.MaxLivestream,
                MaxProduct = existingMembership.MaxProduct,
                Name = existingMembership.Name,
                Type = existingMembership.Type.ToString(),
                UpdatedAt = existingMembership.LastModifiedAt,
                UpdatedBy = existingMembership.LastModifiedBy,
                IsDeleted = existingMembership.IsDeleted,
            };
            return result;
        }

        public async Task<ApiResponse<DetailMembershipDTO>> UpdateMembership(UpdateMembershipDTO request, string userId, string membershipId)
        {
            var result = new ApiResponse<DetailMembershipDTO>()
            {
                Success = true,
                Message = "Cập nhật gói thành viên thành công"
            };
            var existingMembership = await _membershipRepository.GetByIdAsync(membershipId);
            if (existingMembership == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy gói thành viên";
                return result;
            }
            if (!string.IsNullOrWhiteSpace(request.Name))
                existingMembership.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Description))
                existingMembership.Description = request.Description;

            if (request.Price.HasValue && request.Price.Value > 0)
                existingMembership.Price = request.Price.Value;

            if (request.Duration.HasValue && request.Duration.Value > 0)
                existingMembership.Duration = request.Duration.Value;

            if (request.Commission.HasValue)
                existingMembership.Commission = request.Commission.Value;

            if (request.MaxLivestream.HasValue)
                existingMembership.MaxLivestream = request.MaxLivestream.Value;

            if (request.MaxProduct.HasValue)
                existingMembership.MaxProduct = request.MaxProduct.Value;

            if (request.Type.HasValue)
                existingMembership.Type = request.Type.Value.ToString(); 
            existingMembership.SetModifier(userId);

            try
            {
                await _membershipRepository.ReplaceAsync(existingMembership.Id.ToString(), existingMembership);
                result.Data = new DetailMembershipDTO()
                {
                    MembershipId = existingMembership.Id.ToString(),
                    Price = existingMembership.Price,
                    Description = existingMembership.Description,
                    Commission = existingMembership.Commission,
                    CreatedAt = existingMembership.CreatedAt,
                    CreatedBy = existingMembership.CreatedBy,
                    Duration = existingMembership.Duration,
                    MaxLivestream = existingMembership.MaxLivestream,
                    MaxProduct = existingMembership.MaxProduct,
                    Name = existingMembership.Name,
                    Type = existingMembership.Type.ToString(),
                    UpdatedAt = existingMembership.LastModifiedAt,
                    UpdatedBy = existingMembership.LastModifiedBy,
                    IsDeleted = existingMembership.IsDeleted,
                };
                return result;
            }
            catch (Exception ex)
            {

                result.Success = false;
                result.Message = ex.Message;
                return result;

            }
        }
    }
}
