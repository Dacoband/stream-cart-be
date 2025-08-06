using MediatR;
using ProductService.Application.DTOs.Combinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.CombinationCommands
{
    public class GenerateProductCombinationsCommand : IRequest<bool>
    {
        public Guid ProductId { get; set; }
        public List<AttributeValueGroup> AttributeValueGroups { get; set; } = new List<AttributeValueGroup>();
        public decimal DefaultPrice { get; set; }
        public int DefaultStock { get; set; }
        public string? CreatedBy { get; set; }
        public decimal? Weight { get; set; }
        //public string? Dimensions { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
    }
}
