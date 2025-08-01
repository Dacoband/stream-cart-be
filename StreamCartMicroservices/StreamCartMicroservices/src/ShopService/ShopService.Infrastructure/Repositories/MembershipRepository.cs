﻿
using Microsoft.EntityFrameworkCore;
using Shared.Common.Data.Repositories;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Repositories
{
    public class MembershipRepository : EfCoreGenericRepository<Membership>, IMembershipRepository
    {
        private readonly ShopContext _context;

        public MembershipRepository(ShopContext context) : base(context)
        {
            _context = context;
        }
    }
}
