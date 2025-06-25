using MediatR;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.CategoryCommands
{
    public class CreateCategoryCommand : IRequest<ApiResponse<Category>>
    {

        public string CategoryName { get; set; }

        public string? Description { get; set; }

        public string? IconURL { get; set; }

        public string? Slug { get; set; }

        public Guid? ParentCategoryID { get; set; } = Guid.Empty;
        public string? CreatedBy { get; set; } = string.Empty;
        public string? LastModifiedBy { get;  set; }
        public bool IsDeleted { get;  set; } 

    }
}
