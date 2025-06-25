using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.AddressDTOs
{
    public class GHNDistrictDTO
    {
        
            public int DistrictID { get; set; }
            public int ProvinceID { get; set; }
            public string DistrictName { get; set; }
            public int Code { get; set; }
            public int Type { get; set; }
            public int SupportType { get; set; }
            public List<string> NameExtension { get; set; }
            public bool CanUpdateCOD { get; set; }
            public int Status { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }
        

    }
}
