using ShopService.Domain.Enums;

namespace ShopService.Application.DTOs
{
    public class ShopAccountDTO
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public RoleType Role { get; set; }
    }

    public enum RoleType
    {
        Customer = 0,
        Seller = 1,
        Moderator = 2,
        OperationManager = 3,
        ITAdmin = 4
    }
}