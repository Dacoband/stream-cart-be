using MediatR;
using ProductService.Application.Commands.FlashSaleCommands;
using ProductService.Application.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.FlashSaleHandlers
{
    public class DeleteFlashSaleHandler : IRequestHandler<DeleteFlashSaleCommand, ApiResponse<bool>>
    {
        private readonly IFlashSaleService _flashSaleService;
        public DeleteFlashSaleHandler(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }
        public async Task<ApiResponse<bool>> Handle(DeleteFlashSaleCommand request, CancellationToken cancellationToken)
        {
            return await _flashSaleService.DeleteFlashsale(request.FlashSaleId, request.UserId, request.ShopId);
        }
    }
}
