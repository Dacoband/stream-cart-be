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
using Appwrite.Models;

namespace ShopService.Application.Handlers
{
    public class CreateShopCommandHandler : IRequestHandler<CreateShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IAddressServiceClient _addressServiceClient;

        public CreateShopCommandHandler(IShopRepository shopRepository, IPublishEndpoint publishEndpoint, IAccountServiceClient accountServiceClient, IAddressServiceClient addressServiceClient)
        {
            _shopRepository = shopRepository;
            _publishEndpoint = publishEndpoint;
            _accountServiceClient = accountServiceClient;
            _addressServiceClient = addressServiceClient;
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
            shop.UpdateBankingInfo(request.BankNumber,request.BankName,request.Tax,request.CreatedBy);
            // Set creator
            if (!string.IsNullOrEmpty(request.CreatedBy))
            {
                shop.SetCreator(request.CreatedBy);
            }

            var addressDto = new DTOs.Address.CreateAddressDto()
            {
                City = request.City,
                Country = request.Country,
                District = request.District,
                PhoneNumber = request.PhoneNumber,
                IsDefaultShipping = true,
                PostalCode = request.PostalCode,
                RecipientName = request.ShopName,
                ShopId = shop.Id,
                Street = request.Street,
                Type = DTOs.Address.AddressType.Business,
                Ward = request.Ward,
                
            };
            try
            {
                
                
                await _accountServiceClient.UpdateAccountShopInfoAsync(request.AccountId, shop.Id);
                await _shopRepository.InsertAsync(shop);
                var result = await _addressServiceClient.CreateAddressAsync(addressDto, request.AccessToken);

                if (!result.Success)
                {
                    throw new ArgumentException(result.Message);
                }
                //await _publishEndpoint.Publish(new ShopRegistered
                //{
                //    ShopId = shop.Id,
                //    ShopName = shop.ShopName,
                //    AccountId = request.AccountId,
                //    RegistrationDate = shop.RegistrationDate
                //}, cancellationToken);
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
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message.ToString());

            }

            // Return DTO
        }
    }
}