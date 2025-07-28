using Microsoft.EntityFrameworkCore.Storage;
using ShopService.Application.Interfaces;
using ShopService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Repositories
{
    public class ShopUnitOfWork : IShopUnitOfWork
    {
        private readonly ShopContext _context;
        private IDbContextTransaction _transaction;

        public ShopUnitOfWork(ShopContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.SaveChangesAsync(); // đảm bảo context lưu
            await _transaction?.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction?.RollbackAsync();
        }
    }
}
