using MediatR;
using ProductService.Application.DTOs.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.ImageCommands
{
    public class ReorderProductImagesCommand : IRequest<bool>
    {
        public List<ImageOrderItem>? ImagesOrder { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
