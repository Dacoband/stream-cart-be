using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.ImageCommands
{
    public class SetPrimaryImageCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool IsPrimary { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
