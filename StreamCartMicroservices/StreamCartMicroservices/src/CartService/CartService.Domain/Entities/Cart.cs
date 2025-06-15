using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Entities
{
    public class Cart : BaseEntity
    {
        [Required]
        [ForeignKey("Account")]
        public Guid CustomerId { get; private set; }

    }
}
