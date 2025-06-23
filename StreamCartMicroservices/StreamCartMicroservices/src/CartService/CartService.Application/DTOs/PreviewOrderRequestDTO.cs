using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class PreviewOrderRequestDTO
    {
        public List<Guid> CartItemId { get; set; }
    }
}
