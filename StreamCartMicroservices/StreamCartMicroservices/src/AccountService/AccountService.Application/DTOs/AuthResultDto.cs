namespace AccountService.Application.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public AccountDto? Account { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}