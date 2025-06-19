using MediatR;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Events;
using ShopService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers
{
    public class ApproveShopCommandHandler : IRequestHandler<ApproveShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMessagePublisher _messagePublisher;

        public ApproveShopCommandHandler(IShopRepository shopRepository, IMessagePublisher messagePublisher)
        {
            _shopRepository = shopRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<ShopDto> Handle(ApproveShopCommand request, CancellationToken cancellationToken)
        {
            // Get shop by ID
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.ShopId} not found");
            }

            // Set approver
            if (!string.IsNullOrEmpty(request.ApprovedBy))
            {
                shop.SetModifier(request.ApprovedBy);
            }

            // Approve shop
            shop.Approve();

            // Save changes
            await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

            // Publish event
            await _messagePublisher.PublishAsync(new ShopApproved
            {
                ShopId = shop.Id,
                ShopName = shop.ShopName,
                AccountId = Guid.Empty, 
                ApprovalDate = shop.ApprovalDate ?? DateTime.UtcNow
            }, cancellationToken);

            // Return DTO
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