using MassTransit;
using MediatR;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Events;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers
{
    public class UpdateRejectedShopCommandHandler : IRequestHandler<UpdateRejectedShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IAddressServiceClient _addressServiceClient;
        public UpdateRejectedShopCommandHandler(IShopRepository shopRepository, IPublishEndpoint publishEndpoint, IAccountServiceClient accountServiceClient, IAddressServiceClient addressServiceClient)
        {
            _shopRepository = shopRepository;
            _publishEndpoint = publishEndpoint;
            _accountServiceClient = accountServiceClient;
            _addressServiceClient = addressServiceClient;
        }
        public async Task<ShopDto> Handle(UpdateRejectedShopCommand request, CancellationToken cancellationToken)
        {
            

            // Check if shop name is unique
            var isUnique = await _shopRepository.IsNameUniqueAsync(request.ShopName);
            if (!isUnique)
            {
                throw new InvalidOperationException($"A shop with name '{request.ShopName}' already exists");
            }

            // Create shop entity
            var shop = await _shopRepository.GetByIdAsync(request.Id.ToString());

            // Set creator
            if (!string.IsNullOrEmpty(request.CreatedBy))
            {
                shop.SetModifier(request.CreatedBy);
            }
            shop.UpdateBasicInfo(
                request.ShopName, request.Description, request.LogoURL, request.CoverImageURL, request.CreatedBy
                );
            shop.UpdateBankingInfo(request.BankNumber,request.BankName,request.Tax,request.CreatedBy);
            var exsitingAddressList = await _addressServiceClient.GetAddressesByShopIdAsync(shop.Id);
            var existingAddress = exsitingAddressList.Data?.FirstOrDefault();
            var addressDto = new DTOs.Address.CreateAddressDto()
            {
                City = request.City ?? existingAddress.City,
                Country = request.Country ?? existingAddress.Country,
                District = request.District ?? existingAddress.District,
                PhoneNumber = request.PhoneNumber ?? existingAddress.PhoneNumber,
                IsDefaultShipping = true,
                PostalCode = request.PostalCode ?? existingAddress.PostalCode,
                RecipientName = request.ShopName ?? existingAddress.RecipientName,
                ShopId = shop.Id ,
                Street = request.Street ?? existingAddress.Street,
                Type = DTOs.Address.AddressType.Business,
                Ward = request.Ward ?? existingAddress.Ward,
            };
            try
            {


                await _shopRepository.ReplaceAsync(request.Id.ToString(), shop);
                var result = await _addressServiceClient.UpdateAddressAsync(request.Id, addressDto, request.AccessToken);

                if (!result.Success)
                {
                    throw new ArgumentException(result.Message);
                }
                //await _publishEndpoint.Publish(new ShopRegistered
                //{
                //    ShopId = shop.Id,
                //    ShopName = shop.ShopName,
                //    AccountId = Guid.Parse( request.CreatedBy),
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
        }
    }
}
