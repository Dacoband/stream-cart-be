using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.CategoryCommands
{
    public class DeleteCategoryCommand : IRequest<ApiResponse<bool>>
    {
        public Guid CategoryId { get; set; }
        public string Modifier { get; set; }
    }
}
