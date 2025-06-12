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
    public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, bool>
    {
        private readonly IAddressRepository _addressRepository;

        public DeleteAddressCommandHandler(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }

        public async Task<bool> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.Id.ToString());
            if (address == null)
            {
                throw new ApplicationException($"Address with ID {request.Id} not found");
            }

            if (address.AccountId != request.AccountId)
            {
                throw new UnauthorizedAccessException($"Address with ID {request.Id} does not belong to account {request.AccountId}");
            }

            address.SetUpdatedBy(request.DeletedBy);

            address.Delete();

            await _addressRepository.ReplaceAsync(address.Id.ToString(), address);

            return true;
        }
    }
}
