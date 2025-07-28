using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class ApplyFlashSaleCommand : IRequest<LivestreamProductDTO>
    {
        public Guid Id { get; set; }
        //public Guid FlashSaleId { get; set; }
        public decimal Price { get; set; }
        public Guid SellerId { get; set; }
    }
}
