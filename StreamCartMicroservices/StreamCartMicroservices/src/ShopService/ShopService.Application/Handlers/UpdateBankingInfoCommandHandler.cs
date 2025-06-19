using MediatR;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers
{
    public class UpdateBankingInfoCommandHandler : IRequestHandler<UpdateBankingInfoCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;

        public UpdateBankingInfoCommandHandler(IShopRepository shopRepository)
        {
            _shopRepository = shopRepository;
        }

        public async Task<ShopDto> Handle(UpdateBankingInfoCommand request, CancellationToken cancellationToken)
        {
            // Tìm shop theo ID
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.ShopId} not found");
            }

            // Cập nhật thông tin ngân hàng
            shop.UpdateBankingInfo(
                request.BankAccountNumber,
                request.BankName,
                request.TaxNumber,
                request.UpdatedBy
            );

            // Lưu thay đổi
            await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

            // Trả về DTO
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