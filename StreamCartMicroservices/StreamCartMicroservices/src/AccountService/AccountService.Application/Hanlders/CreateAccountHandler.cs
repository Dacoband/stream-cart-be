using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Domain.Entities;
using AccountService.Infrastructure.Interfaces;
using AccountService.Infrastructure.Messaging.Events;
using MassTransit;
using MediatR;
using System.Threading;

namespace AccountService.Application.Hanlders
{
    public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, AccountDto>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateAccountHandler(IAccountRepository accountRepository, IPublishEndpoint publishEndpoint)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            // Check if username or email already exists
            bool isUsernameUnique = await _accountRepository.IsUsernameUniqueAsync(request.Username);
            if (!isUsernameUnique)
            {
                throw new InvalidOperationException("Username is already taken");
            }

            bool isEmailUnique = await _accountRepository.IsEmailUniqueAsync(request.Email);
            if (!isEmailUnique)
            {
                throw new InvalidOperationException("Email is already registered");
            }

            // Create a new account
            var account = new Account(
                request.Username,
                request.Password, // In a real application, this should be hashed
                request.Email,
                request.Role
            );

            // Add additional properties if provided
            if (!string.IsNullOrEmpty(request.PhoneNumber) || 
                !string.IsNullOrEmpty(request.Fullname) || 
                !string.IsNullOrEmpty(request.AvatarURL))
            {
                account.UpdateProfile(request.Fullname, request.PhoneNumber, request.AvatarURL);
            }

            // Set the creator
            account.SetCreator(request.Username);

            // Save to repository
            await _accountRepository.InsertAsync(account);

            // Publish event
            await _publishEndpoint.Publish(new AccountRegistered
            {
                AccountId = account.Id,
                Username = account.Username,
                Email = account.Email,
                Role = account.Role.ToString(),
                RegistrationDate = account.RegistrationDate
            }, cancellationToken);

            // Map to DTO and return
            return new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                Fullname = account.Fullname,
                AvatarURL = account.AvatarURL,
                Role = account.Role,
                RegistrationDate = account.RegistrationDate,
                LastLoginDate = account.LastLoginDate,
                IsActive = account.IsActive,
                IsVerified = account.IsVerified,
                CompleteRate = account.CompleteRate,
                ShopId = account.ShopId,
                CreatedAt = account.CreatedAt,
                CreatedBy = account.CreatedBy,
                LastModifiedAt = account.LastModifiedAt,
                LastModifiedBy = account.LastModifiedBy
            };
        }
    }
}
