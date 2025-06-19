using MediatR;
using ShopService.Application.DTOs;
using ShopService.Application.Queries;
using Shared.Common.Domain.Bases;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShopService.Application.Interfaces;

namespace ShopService.Application.Handlers
{
    public class SearchShopsQueryHandler : IRequestHandler<SearchShopsQuery, PagedResult<ShopDto>>
    {
        private readonly IShopRepository _shopRepository;

        public SearchShopsQueryHandler(IShopRepository shopRepository)
        {
            _shopRepository = shopRepository;
        }

        public async Task<PagedResult<ShopDto>> Handle(SearchShopsQuery request, CancellationToken cancellationToken)
        {
            var result = await _shopRepository.GetPagedShopsAsync(
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.ApprovalStatus,
                request.SearchTerm,
                request.SortBy,
                request.Ascending);

            var shopDtos = result.Items.Select(shop => new ShopDto
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
                Status = shop.Status == ShopService.Domain.Enums.ShopStatus.Active,
                CreatedAt = shop.CreatedAt,
                CreatedBy = shop.CreatedBy,
                LastModifiedAt = shop.LastModifiedAt,
                LastModifiedBy = shop.LastModifiedBy
            }).ToList();

            // Sử dụng constructor thay vì object initializer
            return new PagedResult<ShopDto>(
                shopDtos,
                result.TotalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}