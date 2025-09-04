using MediatR;
using OrderService.Application.DTOs.OrderDTOs;
using System;

namespace OrderService.Application.Queries.OrderQueries
{
    /// <summary>
    /// Query để lấy doanh thu và sản phẩm của livestream
    /// </summary>
    public class GetLivestreamRevenueQuery : IRequest<LivestreamRevenueDto>
    {
        /// <summary>
        /// ID của livestream
        /// </summary>
        public Guid LivestreamId { get; set; }
    }
}