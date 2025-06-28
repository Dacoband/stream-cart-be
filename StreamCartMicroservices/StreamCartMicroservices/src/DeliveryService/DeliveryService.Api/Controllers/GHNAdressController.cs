using DeliveryService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Api.Controllers
{
    [Route("api/deliveries")]
    [ApiController]
    public class GHNAdressController : ControllerBase
    {
        private readonly IDeliveryAddressInterface _addressService;
        public GHNAdressController(IDeliveryAddressInterface addressService)
        {
            _addressService = addressService;
        }
        [HttpGet("/wards")]
        public async Task<IActionResult> GetWards()
        {
            var provinces = await _addressService.GetProvincesAsync();
            var hcm = provinces.FirstOrDefault(p => p.ProvinceName.Contains("Hồ Chí Minh"));
            if (hcm == null) return NotFound("Không tìm thấy TP.HCM");

            var districts = await _addressService.GetDistrictsAsync(hcm.ProvinceID);
            var q1 = districts.FirstOrDefault(d => d.DistrictName.Contains("Quận 1"));
            if (q1 == null) return NotFound("Không tìm thấy Quận 1");

            var wards = await _addressService.GetWardsAsync(q1.DistrictID);
            return Ok(wards);
        }
    }
}
