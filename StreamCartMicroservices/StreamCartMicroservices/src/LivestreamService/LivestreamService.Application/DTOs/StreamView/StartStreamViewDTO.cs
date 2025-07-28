using System;
using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs.StreamView
{
    public class StartStreamViewDTO
    {
        [Required]
        public Guid LivestreamId { get; set; }
    }
}