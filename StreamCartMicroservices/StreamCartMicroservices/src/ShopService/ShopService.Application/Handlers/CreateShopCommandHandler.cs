using MediatR;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Domain.Entities;
using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;
using ShopService.Application.Interfaces;
using ShopService.Application.Events;

namespace ShopService.Application.Handlers
{
    public class CreateShopCommandHandler : IRequestHandler<CreateShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateShopCommandHandler(IShopRepository shopRepository, IPublishEndpoint publishEndpoint)
        {
            _shopRepository = shopRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ShopDto> Handle(CreateShopCommand request, CancellationToken cancellationToken)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.ShopName))
            {
                throw new ArgumentException("Shop name cannot be empty", nameof(request.ShopName));
            }

            // Check if shop name is unique
            var isUnique = await _shopRepository.IsNameUniqueAsync(request.ShopName);
            if (!isUnique)
            {
                throw new InvalidOperationException($"A shop with name '{request.ShopName}' already exists");
            }

            // Create shop entity
            var shop = new Shop(
                request.ShopName,
                request.Description,
                request.LogoURL,
                request.CoverImageURL
            );

            // Set creator
            if (!string.IsNullOrEmpty(request.CreatedBy))
            {
                shop.SetCreator(request.CreatedBy);
            }

            // Save to database
            await _shopRepository.InsertAsync(shop);

            // Publish event
            await _publishEndpoint.Publish(new ShopRegistered
            {
                ShopId = shop.Id,
                ShopName = shop.ShopName,
                AccountId = request.AccountId,  
                RegistrationDate = shop.RegistrationDate
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