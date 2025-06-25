using MediatR;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using ShopService.Application.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Shared.Messaging.Event.ShopEvent;

namespace ShopService.Application.Handlers
{
    public class UpdateShopCommandHandler : IRequestHandler<UpdateShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateShopCommandHandler(IShopRepository shopRepository, IMessagePublisher messagePublisher, IPublishEndpoint publishEndpoint)
        {
            _shopRepository = shopRepository;
            _messagePublisher = messagePublisher;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ShopDto> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
        {
            // Tìm shop theo ID
            var shop = await _shopRepository.GetByIdAsync(request.Id.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.Id} not found");
            }

            // Kiểm tra tên cửa hàng có bị trùng không nếu thay đổi
            if (!string.IsNullOrWhiteSpace(request.ShopName) && request.ShopName != shop.ShopName)
            {
                bool isNameUnique = await _shopRepository.IsNameUniqueAsync(request.ShopName, request.Id);
                if (!isNameUnique)
                {
                    throw new InvalidOperationException($"Shop name '{request.ShopName}' is already in use");
                }
            }

            // Cập nhật thông tin cơ bản
            shop.UpdateBasicInfo(
                request.ShopName,
                request.Description,
                request.LogoURL,
                request.CoverImageURL,
                request.UpdatedBy
            );

            // Lưu thay đổi
            await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

            // Tạo và publish event thông báo shop đã được cập nhật
            await _messagePublisher.PublishAsync(new ShopUpdated
            {
                ShopId = shop.Id,
                ShopName = shop.ShopName,
                LogoURL = shop.LogoURL,
                CoverImageURL = shop.CoverImageURL,
                LastUpdatedDate = DateTime.UtcNow
            }, cancellationToken);

            var shopUpdatedEvent = new ShopUpdatedEvent()
            {
                ShopId = shop.Id,
                ShopName = shop.ShopName,
            };
            await _publishEndpoint.Publish(shopUpdatedEvent);


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