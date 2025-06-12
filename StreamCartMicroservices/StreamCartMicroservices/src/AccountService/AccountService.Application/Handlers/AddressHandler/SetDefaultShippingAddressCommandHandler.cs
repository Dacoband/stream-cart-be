using AccountService.Application.Commands.AddressCommand;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers.AddressHandler
{
    public class SetDefaultShippingAddressCommandHandler : IRequestHandler<SetDefaultShippingAddressCommand, bool>
    {
        private readonly IAddressRepository _addressRepository;

        public SetDefaultShippingAddressCommandHandler(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }

        public async Task<bool> Handle(SetDefaultShippingAddressCommand request, CancellationToken cancellationToken)
        {
            return await _addressRepository.SetDefaultShippingAddressAsync(request.AddressId.ToString(), request.AccountId);
        }
    }
}
