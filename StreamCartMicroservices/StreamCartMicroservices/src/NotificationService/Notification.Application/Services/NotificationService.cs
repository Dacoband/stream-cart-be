using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastrcture.Interface;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<ApiResponse<Notifications>> CreateNotification(CreateNotificationDTO notification)
        {
            Notifications request = new Notifications();
            request.RecipientUserID = notification.RecipientUserID;
            request.OrderCode = notification.OrderCode;
            request.ProductId = notification.ProductId;
            request.VariantId = notification.VariantId;
            request.LivestreamId = notification.LivestreamId;
            request.Type = "System";
            request.Message = notification.Message;
            request.IsRead = false;

            request.SetCreator("system");
            try
            {
                await _notificationRepository.CreateAsync(request);
                return new ApiResponse<Notifications> { Success = true, Message = "Tạo mới thông báo thành công", Data = request };

            }
            catch (Exception ex) { 
            return new ApiResponse<Notifications> { Success = false, Message = ex.Message };
            
            }
        }

        public async Task<ApiResponse<ListNotificationDTO>> GetMyNotification(FilterNotificationDTO filter, string userId)
        {
            
            var query =  _notificationRepository.GetByUserIdAsync(userId);
            if(query.ToList().Count == 0) return new ApiResponse<ListNotificationDTO>() { Success = false, Message ="Không tìm thấy thông báo" };
            

            if (!string.IsNullOrEmpty(filter.Type))
                query = query.Where(n => n.Type == filter.Type);

            if (filter.IsRead.HasValue)
                query = query.Where(n => n.IsRead == filter.IsRead);

            int pageIndex = filter.PageIndex ?? 1;
            int pageSize = filter.PageSize ?? 10;

            var totalItems =  query.ToList().Count();
            var notifications = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to DTO
            var detail = notifications.Select(n => new DetailNotificationDTO
            {
               NotificationId = n.Id,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                LinkUrl = n.LinkUrl,
                Created = n.CreatedAt,
            }).ToList();
            var result = new ListNotificationDTO()
            {
                TotalItem = totalItems,
                NotificationList = detail,
                PageCount = pageSize,
                PageIndex = pageIndex,
            };

            return new ApiResponse<ListNotificationDTO>()
            {
               Data = result,
               Success = true,
               
            };
        }
        public async Task<ApiResponse<bool>> MarkAsRead(Guid id)
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(id);
                return new ApiResponse<bool>()
                {
                    Success = true,
                    Message = "Đánh dấu thông báo là đã đọc",
                };
            }
            catch (Exception ex) {
                return new ApiResponse<bool>()
                {
                    Success = false,
                    Message = ex.Message,
                };
            
            }
        }
        
    }
}
