using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Commands.AddressCommand
{
    public class SetDefaultShippingAddressCommand : IRequest<bool>
    {
        public Guid AddressId { get; set; }
        public Guid AccountId { get; set; }
    }
}
