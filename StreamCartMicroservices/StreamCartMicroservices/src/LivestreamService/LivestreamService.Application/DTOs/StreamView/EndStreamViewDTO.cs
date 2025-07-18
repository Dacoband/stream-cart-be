using System;
using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs.StreamView
{
    public class EndStreamViewDTO
    {
        [Required]
        public Guid StreamViewId { get; set; }
    }
}