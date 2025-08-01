﻿using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopService.Domain.Entities
{
    public class ShopMembership : BaseEntity
    {
        public Guid MembershipID { get; set; }
        public Guid ShopID { get; set; }
        [Column(TypeName = "timestamp without time zone")]

        public DateTime StartDate { get; set; }
        [Column(TypeName = "timestamp without time zone")]

        public DateTime EndDate { get; set; }
        public int RemainingLivestream { get; set; }
        public string Status {  get; set; }
        public int? MaxProduct { get; set; }
        public decimal? Commission { get; set; }

        [JsonIgnore]
        public Membership Membership { get; set; }
        [JsonIgnore]
        public Shop Shop { get; set; }
    }
}
