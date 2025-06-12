using AccountService.Application.DTOs.Address;
using AccountService.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Queries
{
    public class GetAddressByIdQuery : IRequest<AddressDto>
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
    }

    public class GetAddressesByAccountIdQuery : IRequest<IEnumerable<AddressDto>>
    {
        public Guid AccountId { get; set; }
    }

    public class GetAddressesByShopIdQuery : IRequest<IEnumerable<AddressDto>>
    {
        public Guid ShopId { get; set; }
    }

    public class GetDefaultShippingAddressQuery : IRequest<AddressDto>
    {
        public Guid AccountId { get; set; }
    }

    public class GetAddressesByTypeQuery : IRequest<IEnumerable<AddressDto>>
    {
        public Guid AccountId { get; set; }
        public AddressType Type { get; set; }
    }
}
