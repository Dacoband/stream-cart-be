using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.WalletTransaction
{
    public class ListWalletransationDTO
    {
        public List<ShopService.Domain.Entities.WalletTransaction> Items { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
    }
}
