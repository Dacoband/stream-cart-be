using MediatR;
using ProductService.Application.Commands.FlashSaleCommands;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.FlashSaleHandlers
{
    public class UpdateFlashSaleHandler : IRequestHandler<UpdateFlashSaleCommand, ApiResponse<DetailFlashSaleDTO>>
    {
        private readonly IFlashSaleService _flashSaleService;
        public UpdateFlashSaleHandler(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }
        public async Task<ApiResponse<DetailFlashSaleDTO>> Handle(UpdateFlashSaleCommand request, CancellationToken cancellationToken)
        {
            UpdateFlashSaleDTO updateFlashSaleDTO = new UpdateFlashSaleDTO()
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                FLashSalePrice = request.FLashSalePrice,
                QuantityAvailable = request.QuantityAvailable,
            };
            return await _flashSaleService.UpdateFlashSale(updateFlashSaleDTO, request.FlashSaleId, request.UserId, request.ShopId);
        }
    }
}
