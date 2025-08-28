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
    //public class CreateFlashSaleHandle : IRequestHandler<CreateFlashSaleCommand, ApiResponse<List<DetailFlashSaleDTO>>>
    //{
    //    private readonly IFlashSaleService _flashSaleService;
    //    public CreateFlashSaleHandle(IFlashSaleService flashSaleService)
    //    {
    //        _flashSaleService = flashSaleService;
    //    }

    //    //public async Task<ApiResponse<List<DetailFlashSaleDTO>>> Handle(CreateFlashSaleCommand request, CancellationToken cancellationToken)
    //    //{
    //    //    CreateFlashSaleDTO createFlashSaleDTO = new CreateFlashSaleDTO()
    //    //    {
    //    //        ProductId = request.ProductId,
    //    //        VariantId = request.VariantId,
    //    //        StartTime = request.StartTime,
    //    //        EndTime = request.EndTime,
    //    //        QuantityAvailable = request.QuantityAvailable,
    //    //        FLashSalePrice = request.FLashSalePrice,
    //    //    };
    //    //    return await _flashSaleService.CreateFlashSale(createFlashSaleDTO, request.UserId, request.ShopId);
    //    //}
    //}
}
