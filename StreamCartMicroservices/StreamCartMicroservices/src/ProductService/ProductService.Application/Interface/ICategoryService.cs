using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interface
{
    public interface ICategoryService
    {
        public Task<ApiResponse<Category>> CreateCategory(Category category);   
    }
}
