using OrderService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    public interface IAdressServiceClient
    {
        Task<AdressDto> GetCustomerAddress(string id);
    }
}
