using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace LivestreamService.Application.Queries
{
    public class GetLivestreamProductBySkuQuery : IRequest<LivestreamProductDTO?>
    {
        public Guid LivestreamId { get; set; }
        public string Sku { get; set; } = string.Empty;
    }

    public class GetLivestreamProductsBySkusQuery : IRequest<IEnumerable<LivestreamProductDTO>>
    {
        public Guid LivestreamId { get; set; }
        public IEnumerable<string> Skus { get; set; } = new List<string>();
    }
}