using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.FlashSaleCommands
{
    public class DeleteFlashSaleCommand : IRequest<ApiResponse<bool>>
    {
        public string FlashSaleId { get; set; }
        public string UserId { get; set; }
        public string ShopId {  get; set; }
    }
}
