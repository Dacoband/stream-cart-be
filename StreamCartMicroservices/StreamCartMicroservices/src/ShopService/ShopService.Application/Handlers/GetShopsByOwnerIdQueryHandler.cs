using MediatR;
using ShopService.Application.DTOs;
using ShopService.Application.Queries;
using ShopService.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers
{
    public class GetShopsByOwnerIdQueryHandler : IRequestHandler<GetShopsByOwnerIdQuery, IEnumerable<ShopDto>>
    {
        private readonly IShopRepository _shopRepository;

        public GetShopsByOwnerIdQueryHandler(IShopRepository shopRepository)
        {
            _shopRepository = shopRepository;
        }

        public async Task<IEnumerable<ShopDto>> Handle(GetShopsByOwnerIdQuery request, CancellationToken cancellationToken)
        {
            var shops = await _shopRepository.GetShopsByAccountIdAsync(request.AccountId);

            return shops.Select(shop => new ShopDto
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
                LastModifiedBy = shop.LastModifiedBy,
                AccountId = request.AccountId 
            }).ToList();
        }
    }
}