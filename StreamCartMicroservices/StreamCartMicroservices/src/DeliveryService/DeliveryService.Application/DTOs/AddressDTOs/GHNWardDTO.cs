using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.AddressDTOs
{
    public class GHNWardDTO
    {
        public string WardCode { get; set; }
        public string DistrictID { get; set; }
        public string WardName { get; set; }
        public List<string> NameExtension { get; set; }
        public bool CanUpdateCOD { get; set; }
        public int SupportType { get; set; }  // 0–3
        public int Status { get; set; }       // 1 (mở), 2 (khóa)
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
