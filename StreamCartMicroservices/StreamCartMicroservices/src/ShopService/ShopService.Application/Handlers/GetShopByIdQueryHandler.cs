using MediatR;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers
{
    public class GetShopByIdQueryHandler : IRequestHandler<GetShopByIdQuery, ShopDto>
    {
        private readonly IShopRepository _shopRepository;

        public GetShopByIdQueryHandler(IShopRepository shopRepository)
        {
            _shopRepository = shopRepository;
        }

        public async Task<ShopDto> Handle(GetShopByIdQuery request, CancellationToken cancellationToken)
        {
            var shop = await _shopRepository.GetByIdAsync(request.Id.ToString());
            if (shop == null)
            {
                return null;
            }

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
                Status = shop.Status == ShopService.Domain.Enums.ShopStatus.Active,
                CreatedAt = shop.CreatedAt,
                CreatedBy = shop.CreatedBy,
                LastModifiedAt = shop.LastModifiedAt,
                LastModifiedBy = shop.LastModifiedBy
            };
        }
    }
}