using AccountService.Application.Commands;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using Shared.Common.Services.Email;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers
{
    public class GenerateOTPCommandHandler : IRequestHandler<GenerateOTPCommand, string>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;

        public GenerateOTPCommandHandler(
            IAccountRepository accountRepository,
            IEmailService emailService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<string> Handle(GenerateOTPCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());

            if (account == null)
            {
                throw new ApplicationException($"Account with ID {request.AccountId} not found");
            }
            string otp = GenerateOTP();
            account.SetVerificationToken(otp);
            // Thời gian hết hạn OTP (15 phút)
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(15);
            account.SetVerificationTokenExpiry(expiryTime);

            await _accountRepository.ReplaceAsync(account.Id.ToString(), account);

            var htmlBody = $@"
                <h2>Xác nhận tài khoản Stream Cart</h2>
                <p>Xin chào {account.Fullname ?? account.Username},</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản với Stream Cart. Để hoàn tất quá trình đăng ký, vui lòng sử dụng mã OTP sau:</p>
                <h1 style='text-align: center; font-size: 36px; letter-spacing: 5px; color: #3366cc;'>{otp}</h1>
                <p>Mã OTP này sẽ hết hạn sau 15 phút.</p>
                <p>Nếu bạn không thực hiện đăng ký tài khoản, vui lòng bỏ qua email này.</p>
                <p>Trân trọng,<br/>Đội ngũ Stream Cart</p>
            ";

            await _emailService.SendEmailAsync(request.Email, "Mã xác thực đăng ký tài khoản Stream Cart", htmlBody, account.Fullname);

            return otp;
        }

        private string GenerateOTP()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0);

            return (random % 900000 + 100000).ToString();
        }
    }
}