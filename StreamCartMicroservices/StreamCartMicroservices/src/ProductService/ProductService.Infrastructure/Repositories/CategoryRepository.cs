using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Repositories
{
    public class CategoryRepository : EfCoreGenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ProductContext dbContext) : base(dbContext)
        {
            
        }
    }
}
