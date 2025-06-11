using AccountService.Application.Commands;
using AccountService.Application.DTOs;
using AccountService.Domain.Entities;
using AccountService.Infrastructure.Interfaces;
using BCrypt.Net;
using MediatR;
using Shared.Common.Services.Email;
using System;
using System.Threading;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace AccountService.Application.Handlers
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMediator _mediator;
        private readonly IEmailService _emailService;

        public CreateAccountCommandHandler(
            IAccountRepository accountRepository,
            IMediator mediator,
            IEmailService emailService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            string hashedPassword;
            try
            {
                hashedPassword = BC.HashPassword(request.Password);
            }
            catch
            {
                hashedPassword = request.Password;
            }

            // Tạo account mới
            var account = new Account(
                request.Username,
                hashedPassword,
                request.Email,
                request.Role
            );

            // Cập nhật thêm thông tin
            account.UpdateProfile(request.Fullname, request.PhoneNumber, request.AvatarURL);

            // Đặt CompleteRate thành 1.0 (100%) theo mặc định
            account.UpdateCompleteRate(1.0m);

            if (!request.IsActive)
            {
                account.Deactivate();
            }

            // Tài khoản chưa được xác minh cho đến khi OTP được xác nhận
            account.SetUnverified();

            await _accountRepository.InsertAsync(account);

            // Tạo và gửi OTP sau khi tài khoản đã được tạo
            await _mediator.Send(new GenerateOTPCommand
            {
                AccountId = account.Id,
                Email = account.Email
            });

            var accountDto = new AccountDto
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

            return accountDto;
        }
    }
}