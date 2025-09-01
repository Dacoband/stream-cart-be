using Shared.Common.Data.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IMembershipRepository : IGenericRepository<Membership>
    {
        Task<Membership?> GetActiveByIdAsync(string id);
        Task<Membership?> GetById(string id);
        Task<IEnumerable<Membership>> GetAll();
        Task<IEnumerable<Membership>> GetAllActiveAsync();
    }
}
