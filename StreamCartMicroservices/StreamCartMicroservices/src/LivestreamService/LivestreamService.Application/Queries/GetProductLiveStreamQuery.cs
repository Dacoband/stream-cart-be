using Livestreamservice.Application.DTOs;
using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Queries
{
    /// <summary>
    /// Query to get a specific product in livestream with all its variants information
    /// </summary>
    public class GetProductLiveStreamQuery : IRequest<ProductLiveStreamDTO>
    {
        public Guid LivestreamId { get; set; }
        public string ProductId { get; set; }

        public GetProductLiveStreamQuery(Guid livestreamId, string productId)
        {
            LivestreamId = livestreamId;
            ProductId = productId;
        }
    }
}