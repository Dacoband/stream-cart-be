using AccountService.Application.DTOs.Address;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Commands.AddressCommand
{
    public class AssignAddressToShopCommand : IRequest<AddressDto>
    {
        public Guid AddressId { get; set; }
        public Guid ShopId { get; set; }
        public Guid AccountId { get; set; }
    }
}
