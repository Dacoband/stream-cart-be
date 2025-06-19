using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ShopService.Application.DTOs;

namespace ShopService.Application.Commands
{
    public class CreateShopCommand : IRequest<ShopDto>
    {
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoURL { get; set; } = string.Empty;
        public string CoverImageURL { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
