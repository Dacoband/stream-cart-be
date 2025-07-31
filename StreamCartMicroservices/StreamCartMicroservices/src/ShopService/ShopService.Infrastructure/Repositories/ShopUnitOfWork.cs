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
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            await _context.SaveChangesAsync(); // Lưu trước khi commit
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync(); // Clean up
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync(); // Clean up
                _transaction = null;
            }
        }
    }
}
