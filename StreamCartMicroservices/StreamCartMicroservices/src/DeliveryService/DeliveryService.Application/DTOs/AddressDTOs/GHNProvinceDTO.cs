using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.AddressDTOs
{
    public class GHNProvinceDTO
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public int CountryID { get; set; }
        public string Code { get; set; }
        public List<string> NameExtension { get; set; }
        public int IsEnable { get; set; }
        public int RegionID { get; set; }
        public int RegionCPN { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool CanUpdateCOD { get; set; }  // Chuyển từ string "false"/"true" → bool
        public int Status { get; set; }
        public int UpdatedEmployee { get; set; }
        public string UpdatedSource { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
