using MediatR;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using ShopService.Application.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers
{
    public class RejectShopCommandHandler : IRequestHandler<RejectShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMessagePublisher _messagePublisher;

        public RejectShopCommandHandler(IShopRepository shopRepository, IMessagePublisher messagePublisher)
        {
            _shopRepository = shopRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<ShopDto> Handle(RejectShopCommand request, CancellationToken cancellationToken)
        {
            // Tìm shop theo ID
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.ShopId} not found");
            }

            // Set người reject
            if (!string.IsNullOrEmpty(request.RejectedBy))
            {
                shop.SetModifier(request.RejectedBy);
            }

            // Reject shop
            shop.Reject(request.RejectedBy, request.RejectionReason);

            // Lưu thay đổi
            await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

            // Tạo và publish event thông báo shop đã bị reject
            await _messagePublisher.PublishAsync(new ShopRejected
            {
                ShopId = shop.Id,
                ShopName = shop.ShopName,
                Reason = request.RejectionReason,
                RejectionDate = DateTime.UtcNow
            }, cancellationToken);

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