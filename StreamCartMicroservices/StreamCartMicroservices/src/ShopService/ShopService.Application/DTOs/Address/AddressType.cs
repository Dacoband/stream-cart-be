using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Address
{
    public enum AddressType
    {
        Residential,  // Nhà riêng
        Business,     // Cơ sở kinh doanh
        Shipping,     // Địa chỉ giao hàng
        Billing,      // Địa chỉ thanh toán
        Both          // Vừa giao hàng vừa thanh toán
    }
}
